#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
namespace SynicSugar.Samples 
{
    /// <summary>
    /// Mark a method with an integer argument with this to display the argument as an enum popup in the UnityEvent
    /// drawer. Use: [EnumAction(typeof(SomeEnumType))]
    /// From https://forum.unity.com/threads/ability-to-add-enum-argument-to-button-functions.270817
    /// </summary>
    [CustomPropertyDrawer(typeof(UnityEvent), true)]
    public class UnityEventDrawer : PropertyDrawer 
    {
        private readonly Dictionary<string, State> _mStates = new Dictionary<string, State>();

        // Find internal methods with reflection
        private static readonly MethodInfo FindMethod = typeof(UnityEventBase).GetMethod("FindMethod",
            BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Standard,
            new[] { typeof(string), typeof(Type), typeof(PersistentListenerMode), typeof(Type) }, null);

        private static readonly MethodInfo Temp = typeof(GUIContent).GetMethod("Temp",
            BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Standard, new[] { typeof(string) },
            null);

        private static readonly PropertyInfo MixedValueContent = typeof(EditorGUI).GetProperty("mixedValueContent", BindingFlags.NonPublic | BindingFlags.Static);

        private Styles _mStyles;
        private string _mText;
        private UnityEventBase _mDummyEvent;
        private SerializedProperty _mProp;
        private SerializedProperty _mListenersArray;
        private ReorderableList _mReorderableList;
        private int _mLastSelectedIndex;

        private static string GetEventParams(UnityEventBase evt) 
        {
            var method = (MethodInfo)FindMethod.Invoke(evt,
                new object[] { "Invoke", evt.GetType(), PersistentListenerMode.EventDefined, null });
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(" (");
            var array = method.GetParameters().Select(x => x.ParameterType).ToArray();
            for (var index = 0; index < array.Length; ++index)
            {
                stringBuilder.Append(array[index].Name);
                if (index < array.Length - 1)
                    stringBuilder.Append(", ");
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private State GetState(SerializedProperty prop)
        {
            var propertyPath = prop.propertyPath;
            _mStates.TryGetValue(propertyPath, out var state);
            if (state != null) return state;
            state = new State();
            var propertyRelative = prop.FindPropertyRelative("m_PersistentCalls.m_Calls");
            state.MReorderableList =
                new ReorderableList(prop.serializedObject, propertyRelative, false, true, true, true)
                {
                    drawHeaderCallback = DrawEventHeader,
                    drawElementCallback = DrawEventListener,
                    onSelectCallback = SelectEventListener,
                    onReorderCallback = EndDragChild,
                    onAddCallback = AddEventListener,
                    onRemoveCallback = RemoveButton,
                    elementHeight = 43f
                };
            _mStates[propertyPath] = state;
            return state;
        }

        private State RestoreState(SerializedProperty property)
        {
            var state = GetState(property);
            _mListenersArray = state.MReorderableList.serializedProperty;
            _mReorderableList = state.MReorderableList;
            _mLastSelectedIndex = state.LastSelectedIndex;
            _mReorderableList.index = _mLastSelectedIndex;
            return state;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _mProp = property;
            _mText = label.text;
            var state = RestoreState(property);
            OnGUI(position);
            state.LastSelectedIndex = _mLastSelectedIndex;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RestoreState(property);
            var num = 0.0f;
            if (_mReorderableList != null)
                num = _mReorderableList.GetHeight();
            return num;
        }

        private void OnGUI(Rect position)
        {
            if (_mListenersArray is not { isArray: true })
                return;
            _mDummyEvent = GetDummyEvent(_mProp);
            if (_mDummyEvent == null)
                return;
            _mStyles ??= new Styles();
            if (_mReorderableList == null)
                return;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            _mReorderableList.DoList(position);
            EditorGUI.indentLevel = indentLevel;
        }

        protected virtual void DrawEventHeader(Rect headerRect)
        {
            headerRect.height = 16f;
            var text = (!string.IsNullOrEmpty(_mText) ? _mText : "Event") + GetEventParams(_mDummyEvent);
            GUI.Label(headerRect, text);
        }

        private static PersistentListenerMode GetMode(SerializedProperty mode) =>
            (PersistentListenerMode)mode.enumValueIndex;

        private void DrawEventListener(Rect rect, int index, bool isActive, bool isFocused)
        {
            var arrayElementAtIndex = _mListenersArray.GetArrayElementAtIndex(index);
            ++rect.y;
            var rowRects = GetRowRects(rect);
            var position1 = rowRects[0];
            var position2 = rowRects[1];
            var rect1 = rowRects[2];
            var position3 = rowRects[3];
            var propertyRelative1 = arrayElementAtIndex.FindPropertyRelative("m_CallState");
            var propertyRelative2 = arrayElementAtIndex.FindPropertyRelative("m_Mode");
            var propertyRelative3 = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
            var propertyRelative4 = arrayElementAtIndex.FindPropertyRelative("m_Target");
            var propertyRelative5 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            EditorGUI.PropertyField(position1, propertyRelative1, GUIContent.none);
            EditorGUI.BeginChangeCheck();
            GUI.Box(position2, GUIContent.none);
            EditorGUI.PropertyField(position2, propertyRelative4, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
                propertyRelative5.stringValue = null;
            var persistentListenerMode = GetMode(propertyRelative2);
            if (propertyRelative4.objectReferenceValue == null || string.IsNullOrEmpty(propertyRelative5.stringValue))
                persistentListenerMode = PersistentListenerMode.Void;
            var propertyRelative6 = persistentListenerMode switch
            {
                PersistentListenerMode.Object => propertyRelative3.FindPropertyRelative("m_ObjectArgument"),
                PersistentListenerMode.Int => propertyRelative3.FindPropertyRelative("m_IntArgument"),
                PersistentListenerMode.Float => propertyRelative3.FindPropertyRelative("m_FloatArgument"),
                PersistentListenerMode.String => propertyRelative3.FindPropertyRelative("m_StringArgument"),
                PersistentListenerMode.Bool => propertyRelative3.FindPropertyRelative("m_BoolArgument"),
                _ => propertyRelative3.FindPropertyRelative("m_IntArgument")
            };
            var stringValue = propertyRelative3.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue;
            var type = typeof(Object);
            if (!string.IsNullOrEmpty(stringValue))
                type = Type.GetType(stringValue, false) ?? typeof(Object);
            if (persistentListenerMode == PersistentListenerMode.Object)
            {
                EditorGUI.BeginChangeCheck();
                var @object = EditorGUI.ObjectField(position3, GUIContent.none, propertyRelative6.objectReferenceValue,
                    type, true);
                if (EditorGUI.EndChangeCheck())
                    propertyRelative6.objectReferenceValue = @object;
            }
            else if (persistentListenerMode != PersistentListenerMode.Void &&
                        persistentListenerMode != PersistentListenerMode.EventDefined &&
                        !propertyRelative6.serializedObject.isEditingMultipleObjects)
            {
                // Try to find Find the EnumActionAttribute
                var method = GetMethod(_mDummyEvent, propertyRelative5.stringValue,
                    propertyRelative4.objectReferenceValue,
                    GetMode(propertyRelative2), type);
                object[] attributes = null;
                if (method != null)
                    attributes = method.GetCustomAttributes(typeof(EnumActionAttribute), true);
                if (attributes is { Length: > 0 })
                {
                    // Make an enum popup
                    var enumType = ((EnumActionAttribute)attributes[0]).EnumType;
                    var value = (Enum)Enum.ToObject(enumType, propertyRelative6.intValue);
                    propertyRelative6.intValue = Convert.ToInt32(EditorGUI.EnumPopup(position3, value));
                }
                else
                    EditorGUI.PropertyField(position3, propertyRelative6, GUIContent.none);
            }

            EditorGUI.BeginDisabledGroup(propertyRelative4.objectReferenceValue == null);
            EditorGUI.BeginProperty(rect1, GUIContent.none, propertyRelative5);
            GUIContent content;
            if (EditorGUI.showMixedValue)
            {
                content = (GUIContent)MixedValueContent.GetValue(null, null);
            }
            else
            {
                var stringBuilder = new StringBuilder();
                if (propertyRelative4.objectReferenceValue == null ||
                    string.IsNullOrEmpty(propertyRelative5.stringValue))
                    stringBuilder.Append("No Function");
                else if (!IsPersistentListenerValid(_mDummyEvent, propertyRelative5.stringValue,
                                propertyRelative4.objectReferenceValue, GetMode(propertyRelative2), type))
                {
                    var str = "UnknownComponent";
                    var objectReferenceValue = propertyRelative4.objectReferenceValue;
                    if (objectReferenceValue != null)
                        str = objectReferenceValue.GetType().Name;
                    stringBuilder.Append($"<Missing {str}.{propertyRelative5.stringValue}>");
                }
                else
                {
                    stringBuilder.Append(propertyRelative4.objectReferenceValue.GetType().Name);
                    if (!string.IsNullOrEmpty(propertyRelative5.stringValue))
                    {
                        stringBuilder.Append(".");
                        stringBuilder.Append(propertyRelative5.stringValue.StartsWith("set_")
                            ? propertyRelative5.stringValue.Substring(4)
                            : propertyRelative5.stringValue);
                    }
                }

                content = (GUIContent)Temp.Invoke(null, new object[] { stringBuilder.ToString() });
            }

            if (GUI.Button(rect1, content, EditorStyles.popup))
                BuildPopupList(propertyRelative4.objectReferenceValue, _mDummyEvent, arrayElementAtIndex)
                    .DropDown(rect1);
            EditorGUI.EndProperty();
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = backgroundColor;
        }

        private static Rect[] GetRowRects(Rect rect)
        {
            var rectArray = new Rect[4];
            rect.height = 16f;
            rect.y += 2f;
            var rect1 = rect;
            rect1.width *= 0.3f;
            var rect2 = rect1;
            rect2.y += EditorGUIUtility.singleLineHeight + 2f;
            var rect3 = rect;
            rect3.xMin = rect2.xMax + 5f;
            var rect4 = rect3;
            rect4.y += EditorGUIUtility.singleLineHeight + 2f;
            rectArray[0] = rect1;
            rectArray[1] = rect2;
            rectArray[2] = rect3;
            rectArray[3] = rect4;
            return rectArray;
        }

        private void RemoveButton(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            _mLastSelectedIndex = list.index;
        }

        private void AddEventListener(ReorderableList list)
        {
            if (_mListenersArray.hasMultipleDifferentValues)
            {
                foreach (var targetObject in _mListenersArray.serializedObject.targetObjects)
                {
                    var serializedObject = new SerializedObject(targetObject);
                    ++serializedObject.FindProperty(_mListenersArray.propertyPath).arraySize;
                    serializedObject.ApplyModifiedProperties();
                }

                _mListenersArray.serializedObject.SetIsDifferentCacheDirty();
                _mListenersArray.serializedObject.Update();
                list.index = list.serializedProperty.arraySize - 1;
            }
            else
                ReorderableList.defaultBehaviours.DoAddButton(list);

            _mLastSelectedIndex = list.index;
            var arrayElementAtIndex = _mListenersArray.GetArrayElementAtIndex(list.index);
            var propertyRelative1 = arrayElementAtIndex.FindPropertyRelative("m_CallState");
            var propertyRelative2 = arrayElementAtIndex.FindPropertyRelative("m_Target");
            var propertyRelative3 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
            var propertyRelative4 = arrayElementAtIndex.FindPropertyRelative("m_Mode");
            var propertyRelative5 = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
            propertyRelative1.enumValueIndex = 2;
            propertyRelative2.objectReferenceValue = null;
            propertyRelative3.stringValue = null;
            propertyRelative4.enumValueIndex = 1;
            propertyRelative5.FindPropertyRelative("m_FloatArgument").floatValue = 0.0f;
            propertyRelative5.FindPropertyRelative("m_IntArgument").intValue = 0;
            propertyRelative5.FindPropertyRelative("m_ObjectArgument").objectReferenceValue = null;
            propertyRelative5.FindPropertyRelative("m_StringArgument").stringValue = null;
            propertyRelative5.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue = null;
        }

        private void SelectEventListener(ReorderableList list) => _mLastSelectedIndex = list.index;

        private void EndDragChild(ReorderableList list) => _mLastSelectedIndex = list.index;

        private static UnityEventBase GetDummyEvent(SerializedProperty _) => new UnityEvent();

        private static IEnumerable<ValidMethodMap> CalculateMethodMap(Object target, IReadOnlyList<Type> t,
            bool allowSubclasses)
        {
            var validMethodMapList = new List<ValidMethodMap>();
            if (target == null || t == null)
                return validMethodMapList;
            var type = target.GetType();
            var list = type.GetMethods().Where(x => !x.IsSpecialName).ToList();
            var source = type.GetProperties().AsEnumerable().Where(x =>
            {
                if (x.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0)
                    return x.GetSetMethod() != null;
                return false;
            });
            list.AddRange(source.Select(x => x.GetSetMethod()));
            using var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var parameters = current!.GetParameters();
                if (parameters.Length != t.Count ||
                    current.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0 ||
                    current.ReturnType != typeof(void)) continue;
                var flag = true;
                for (var index = 0; index < t.Count; ++index)
                {
                    if (!parameters[index].ParameterType.IsAssignableFrom(t[index]))
                        flag = false;
                    if (allowSubclasses && t[index].IsAssignableFrom(parameters[index].ParameterType))
                        flag = true;
                }

                if (flag)
                    validMethodMapList.Add(new ValidMethodMap
                    {
                        Target = target,
                        MethodInfo = current
                    });
            }

            return validMethodMapList;
        }

        public static bool IsPersistentListenerValid(UnityEventBase dummyEvent, string methodName, Object uObject,
            PersistentListenerMode modeEnum, Type argumentType)
        {
            if (uObject == null || string.IsNullOrEmpty(methodName))
                return false;
            return GetMethod(dummyEvent, methodName, uObject, modeEnum, argumentType) != null;
        }

        private static MethodInfo GetMethod(UnityEventBase dummyEvent, string methodName, Object uObject,
            PersistentListenerMode modeEnum, Type argumentType) => (MethodInfo)FindMethod.Invoke(dummyEvent,
            new object[] { methodName, uObject.GetType(), modeEnum, argumentType });

        private static GenericMenu BuildPopupList(Object target, UnityEventBase dummyEvent, SerializedProperty listener)
        {
            var target1 = target;
            if (target1 is Component targetComp)
                target1 = targetComp.gameObject;
            var propertyRelative = listener.FindPropertyRelative("m_MethodName");
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("No Function"), string.IsNullOrEmpty(propertyRelative.stringValue),
                ClearEventFunction, new UnityEventFunction(listener, null, null, PersistentListenerMode.EventDefined));
            if (target1 == null)
                return menu;
            menu.AddSeparator(string.Empty);
            var array = dummyEvent.GetType().GetMethod("Invoke")!.GetParameters().Select(x => x.ParameterType).ToArray();
            GeneratePopUpForType(menu, target1, false, listener, array);
            if (target1 is GameObject gameObject)
            {
                var components = gameObject.GetComponents<Component>();
                var list = components.Where(c => c != null).Select(c => c.GetType().Name).GroupBy(x => x)
                    .Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                foreach (var component in components)
                {
                    if (!(component == null))
                        GeneratePopUpForType(menu, component, list.Contains(component.GetType().Name), listener, array);
                }
            }

            return menu;
        }

        private static void GeneratePopUpForType(GenericMenu menu, Object target, bool useFullTargetName,
            SerializedProperty listener, Type[] delegateArgumentsTypes)
        {
            var methods = new List<ValidMethodMap>();
            var targetName = !useFullTargetName ? target.GetType().Name : target.GetType().FullName;
            var flag = false;
            if (delegateArgumentsTypes.Length != 0)
            {
                GetMethodsForTargetAndMode(target, delegateArgumentsTypes, methods,
                    PersistentListenerMode.EventDefined);
                if (methods.Count > 0)
                {
                    menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " +
                                                        string.Join(", ",
                                                            delegateArgumentsTypes.Select(GetTypeName).ToArray())));
                    AddMethodsToMenu(menu, listener, methods, targetName);
                    flag = true;
                }
            }

            methods.Clear();
            GetMethodsForTargetAndMode(target, new[] { typeof(float) }, methods, PersistentListenerMode.Float);
            GetMethodsForTargetAndMode(target, new[] { typeof(int) }, methods, PersistentListenerMode.Int);
            GetMethodsForTargetAndMode(target, new[] { typeof(string) }, methods, PersistentListenerMode.String);
            GetMethodsForTargetAndMode(target, new[] { typeof(bool) }, methods, PersistentListenerMode.Bool);
            GetMethodsForTargetAndMode(target, new[] { typeof(Object) }, methods, PersistentListenerMode.Object, true);
            GetMethodsForTargetAndMode(target, Type.EmptyTypes, methods, PersistentListenerMode.Void);
            if (methods.Count <= 0)
                return;
            if (flag)
                menu.AddItem(new GUIContent(targetName + "/ "), false, null);
            if (delegateArgumentsTypes.Length != 0)
                menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
            AddMethodsToMenu(menu, listener, methods, targetName);
        }

        static void AddMethodsToMenu(GenericMenu menu, SerializedProperty listener,
            List<ValidMethodMap> methods,
            string targetName)
        {
            foreach (var method in methods.OrderBy(e => e.MethodInfo.Name.StartsWith("set_") ? 0 : 1)
                            .ThenBy(e => e.MethodInfo.Name))
                AddFunctionsForScript(menu, listener, method, targetName);
        }

        private static void GetMethodsForTargetAndMode(Object target, Type[] delegateArgumentsTypes,
            List<ValidMethodMap> methods, PersistentListenerMode mode, bool allowSubclasses = false)
        {
            var methodMaps = CalculateMethodMap(target, delegateArgumentsTypes, allowSubclasses).ToArray();
            for (var i = 0; i < methodMaps.Length; i++)
            {
                methodMaps[i].Mode = mode;
                methods.Add(methodMaps[i]);
            }
        }

        private static void AddFunctionsForScript(GenericMenu menu, SerializedProperty listener, ValidMethodMap method,
            string targetName)
        {
            var mode1 = method.Mode;
            var objectReferenceValue = listener.FindPropertyRelative("m_Target").objectReferenceValue;
            var stringValue = listener.FindPropertyRelative("m_MethodName").stringValue;
            var mode2 = GetMode(listener.FindPropertyRelative("m_Mode"));
            var propertyRelative = listener.FindPropertyRelative("m_Arguments")
                .FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
            var stringBuilder = new StringBuilder();
            var length = method.MethodInfo.GetParameters().Length;
            for (var index = 0; index < length; ++index)
            {
                var parameter = method.MethodInfo.GetParameters()[index];
                stringBuilder.Append(GetTypeName(parameter.ParameterType));
                if (index < length - 1)
                    stringBuilder.Append(", ");
            }

            var on = objectReferenceValue == method.Target && stringValue == method.MethodInfo.Name && mode1 == mode2;
            if (on && mode1 == PersistentListenerMode.Object && method.MethodInfo.GetParameters().Length == 1)
                on &= method.MethodInfo.GetParameters()[0].ParameterType.AssemblyQualifiedName ==
                        propertyRelative.stringValue;
            var formattedMethodName = GetFormattedMethodName(targetName, method.MethodInfo.Name,
                stringBuilder.ToString(),
                mode1 == PersistentListenerMode.EventDefined);
            menu.AddItem(new GUIContent(formattedMethodName), on, SetEventFunction,
                new UnityEventFunction(listener, method.Target, method.MethodInfo, mode1));
        }

        private static string GetTypeName(Type t)
        {
            if (t == typeof(int))
                return "int";
            if (t == typeof(float))
                return "float";
            if (t == typeof(string))
                return "string";
            if (t == typeof(bool))
                return "bool";
            return t.Name;
        }

        private static string GetFormattedMethodName(string targetName, string methodName, string args, bool dynamic)
        {
            if (dynamic)
            {
                return methodName.StartsWith("set_")
                    ? $"{targetName}/{methodName.Substring(4)}"
                    : $"{targetName}/{methodName}";
            }

            return methodName.StartsWith("set_")
                ? $"{targetName}/{args} {methodName.Substring(4)}"
                : $"{targetName}/{methodName} ({args})";
        }

        private static void SetEventFunction(object source) => ((UnityEventFunction)source).Assign();

        private static void ClearEventFunction(object source) => ((UnityEventFunction)source).Clear();

        protected class State
        {
            internal ReorderableList MReorderableList;
            public int LastSelectedIndex;
        }

        private class Styles
        {
            public readonly GUIContent IconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus");
            public readonly GUIStyle GenericFieldStyle = EditorStyles.label;
            public readonly GUIStyle RemoveButton = "InvisibleButton";
        }

        private struct ValidMethodMap
        {
            public Object Target;
            public MethodInfo MethodInfo;
            public PersistentListenerMode Mode;
        }

        private readonly struct UnityEventFunction
        {
            readonly SerializedProperty _mListener;
            readonly Object _mTarget;
            readonly MethodInfo _mMethod;
            readonly PersistentListenerMode _mMode;

            public UnityEventFunction(SerializedProperty listener, Object target, MethodInfo method,
                PersistentListenerMode mode)
            {
                _mListener = listener;
                _mTarget = target;
                _mMethod = method;
                _mMode = mode;
            }

            public void Assign()
            {
                var propertyRelative1 = _mListener.FindPropertyRelative("m_Target");
                var propertyRelative2 = _mListener.FindPropertyRelative("m_MethodName");
                var propertyRelative3 = _mListener.FindPropertyRelative("m_Mode");
                var propertyRelative4 = _mListener.FindPropertyRelative("m_Arguments");
                propertyRelative1.objectReferenceValue = _mTarget;
                propertyRelative2.stringValue = _mMethod.Name;
                propertyRelative3.enumValueIndex = (int)_mMode;
                if (_mMode == PersistentListenerMode.Object)
                {
                    var propertyRelative5 = propertyRelative4.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                    var parameters = _mMethod.GetParameters();
                    propertyRelative5.stringValue =
                        parameters.Length != 1 || !typeof(Object).IsAssignableFrom(parameters[0].ParameterType)
                            ? typeof(Object).AssemblyQualifiedName
                            : parameters[0].ParameterType.AssemblyQualifiedName;
                }

                ValidateObjectParameter(propertyRelative4, _mMode);
                _mListener.serializedObject.ApplyModifiedProperties();
            }

            private static void ValidateObjectParameter(SerializedProperty arguments, PersistentListenerMode mode)
            {
                var propertyRelative1 = arguments.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                var propertyRelative2 = arguments.FindPropertyRelative("m_ObjectArgument");
                var objectReferenceValue = propertyRelative2.objectReferenceValue;
                if (mode != PersistentListenerMode.Object)
                {
                    propertyRelative1.stringValue = typeof(Object).AssemblyQualifiedName;
                    propertyRelative2.objectReferenceValue = null;
                }
                else
                {
                    if (objectReferenceValue == null)
                        return;
                    var type = Type.GetType(propertyRelative1.stringValue, false);
                    if (typeof(Object).IsAssignableFrom(type) && type!.IsInstanceOfType(objectReferenceValue))
                        return;
                    propertyRelative2.objectReferenceValue = null;
                }
            }

            public void Clear()
            {
                _mListener.FindPropertyRelative("m_MethodName").stringValue = null;
                _mListener.FindPropertyRelative("m_Mode").enumValueIndex = 1;
                _mListener.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif