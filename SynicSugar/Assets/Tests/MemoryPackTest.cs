using System.Runtime.InteropServices;
using UnityEngine;
using MemoryPack;
using MemoryPack.Compression;
using SynicSugar.P2P;

namespace SynicSugar.Test {
    public class MemoryPackTest : MonoBehaviour {
        int overwriteTest;
        void Start(){
            MemoryPackTestMain main = MemoryPackTestSetMethod();
            
            Debug.Log($"main: {Marshal.SizeOf(main)}");

            byte[] mainB = MemoryPackSerializer.Serialize(main);
            string mainBC = System.String.Empty;
            foreach(var c in mainB){
                mainBC += c;
            }
            Debug.Log($"mainB: {mainB.Length} / {mainBC}");

            using var compressor  = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor , main);
            byte[] mainC = compressor.ToArray();
            string mainCC = System.String.Empty;
            foreach(var c in mainC){
                mainCC += c;
            }
            Debug.Log($"mainC: {mainC.Length} / {mainCC}");

            
            MemoryPackTestMain mainD = MemoryPackSerializer.Deserialize<MemoryPackTestMain>(mainB);
            mainD.test1.Debug();
            mainD.test2.Debug();
            mainD.test3.Debug();
            mainD.testB.Debug();


            using var decompressor = new BrotliDecompressor();
            var decompressedBuffer = decompressor.Decompress(mainC);
            MemoryPackTestMain mainD2 = MemoryPackSerializer.Deserialize<MemoryPackTestMain>(decompressedBuffer);
            mainD2.test1.Debug();
            mainD2.test2.Debug();
            mainD2.test3.Debug();


            Debug.Log(overwriteTest);
            var testOw = MemoryPackSerializer.Serialize(overwriteTest);
            int owTestD = MemoryPackSerializer.Deserialize<int>(testOw);
            Debug.Log(owTestD);
            MemoryPackSerializer.Deserialize<int>(testOw, ref overwriteTest);
            Debug.Log(overwriteTest);
        }
        MemoryPackTestMain MemoryPackTestSetMethod(){
            MemoryPackTestMain main = new();
            MemoryPackTest1 test1 = new MemoryPackTest1("MemoryPackTest1", 1, 1);
            main.test1 = test1;
            Debug.Log($"test1: {Marshal.SizeOf(test1)}");

            MemoryPackTest2 test2 = new MemoryPackTest2("MemoryPackTest2", 2, 2);
            main.test2 = test2;
            Debug.Log($"test2: {Marshal.SizeOf(test2)}");

            MemoryPackTest3 test3 = new MemoryPackTest3("MemoryPackTest3", 3, GetRandomString(50));
            main.test3 = test3;
            Debug.Log($"test3: {Marshal.SizeOf(test3)}");

            MemoryPackTestBig testB = new MemoryPackTestBig("MemoryPackTestB", 4, GetRandomString(2000));
            main.testB = testB;
            Debug.Log($"testB: {Marshal.SizeOf(testB)}");

            return main;
        }
        string GetRandomString(int length){
            var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var array = new char[length];
            var random = new System.Random();

            for (int i = 0; i < array.Length; i++){
                array[i] = sample[random.Next(sample.Length)];
            }

            return new string(array);
        }
    }
    [MemoryPackable]
    public partial struct MemoryPackTestMain{
        public MemoryPackTest1 test1;
        public MemoryPackTest2 test2;
        public MemoryPackTest3 test3;
        public MemoryPackTestBig testB;
    }
    [MemoryPackable]
    public partial struct MemoryPackTest1{
        public string name;
        public int id;
        public int code;
        public MemoryPackTest1(string name, int id, int code){
            this.name = name;
            this.id = id;
            this.code = code;
        }
        public void Debug(){
            UnityEngine.Debug.Log($"{this.name} / {this.id} / {this.code}");
        }
    }
    [MemoryPackable]
    public partial struct MemoryPackTest2{
        public string name;
        public int id;
        public byte code;
        public MemoryPackTest2(string name, int id, byte code){
            this.name = name;
            this.id = id;
            this.code = code;
        }

        public void Debug(){
            UnityEngine.Debug.Log($"{this.name} / {this.id} / {this.code}");
        }
    }
    [MemoryPackable]
    public partial struct MemoryPackTest3{
        public string name;
        public int id;
        public string code;
        public MemoryPackTest3(string name, int id, string code){
            this.name = name;
            this.id = id;
            this.code = code;
        }

        public void Debug(){
            UnityEngine.Debug.Log($"{this.name} / {this.id} / {this.code}");
        }
    }
    [MemoryPackable]
    public partial struct MemoryPackTestBig{
        public string name;
        public int id;
        public string code;
        public MemoryPackTestBig(string name, int id, string code){
            this.name = name;
            this.id = id;
            this.code = code;
        }

        public void Debug(){
            UnityEngine.Debug.Log($"{this.name} / {this.id} / {this.code}");
        }
    }
}