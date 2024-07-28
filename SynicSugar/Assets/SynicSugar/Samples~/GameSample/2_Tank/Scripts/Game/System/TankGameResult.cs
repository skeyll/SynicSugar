using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SynicSugar.Samples.Tank {
    public class TankGameResult : MonoBehaviour {
        [SerializeField] Text textPrefab;
        [SerializeField] Transform resultsParent;
        List<Text> results;
        /// <summary>
        /// Generate texts to display game result
        /// </summary>
        /// <param name="playerAmount"></param> 
        internal void GenerateResultsText(int playerAmount){
            results = new List<Text>();
            for(int i = 0; i < playerAmount; i++){
                var text = Instantiate(textPrefab, resultsParent);
                results.Add(text);
            }
            results[0].color = Color.red;
        }
        /// <summary>
        /// Insert result and activate result panel.
        /// </summary>
        /// <param name="resultData"></param>
        internal void DisplayResult(List<TankResultData> resultData){
            for(int i = 0; i < resultData.Count; i++){
                results[i].text = $"{i}: {resultData[i].Name}({resultData[i].RemainHP})";
            }

            //ここでクラウンアクティブとカメラ移動？

            gameObject.SetActive(true);
        }
        /// <summary>
        /// Deactivate result panel and reset text.
        /// </summary>
        internal void DeactivateResult(){
            gameObject.SetActive(false);

            foreach(var text in results){
                text.text = string.Empty;
            }
        }
    }
}