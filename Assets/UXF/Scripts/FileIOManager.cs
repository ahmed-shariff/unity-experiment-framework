﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;
using UnityEngine.Events;
using System.Data;


namespace UXF
{
    /// <summary>
    /// Singleton class which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class FileIOManager : MonoBehaviour
    {

        /// <summary>
        /// Queue of actions which gets emptied on each frame in the main thread.
        /// </summary>
        public readonly Queue<System.Action> executeOnMainThreadQueue = new Queue<System.Action>();

        BlockingQueue<System.Action> bq = new BlockingQueue<System.Action>();
        Thread t;

        void Awake()
        {
            t = new Thread(Worker);
            t.Start();
        }

        void Update()
        {
            ManageActions();
        }

        /// <summary>
        /// Adds a new command to a queue which is executed in a separate worker thread when it is available.
        /// </summary>
        /// <param name="command"></param>
        public void Manage(System.Action action)
        {
            bq.Enqueue(action);
        }

        void Worker()
        {
            // performs FileIO tasks in seperate thread
            foreach (var action in bq)
            {
                try
                {
                    action.Invoke();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (IOException e)
                {
                    Debug.LogError(string.Format("Error, file may be in use! Exception: {0}", e));
                }
                catch (System.Exception e)
                {
                    // stops thread aborting
                    Debug.LogError(e);
                }
            }
        }

        public void CopyFile(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public void ReadJSON(string fpath, System.Action<Dictionary<string, object>> callback)
        {
            Dictionary<string, object> dict = null;
            try
            {
                string dataAsJson = File.ReadAllText(fpath);
                dict = MiniJSON.Json.Deserialize(dataAsJson) as Dictionary<string, object>;
            }
            catch (FileNotFoundException)
            {
                string message = string.Format(".json file not found in {0}!", fpath);
                Debug.LogWarning(message);
            }

            System.Action action = new System.Action(() => callback.Invoke(dict));
            executeOnMainThreadQueue.Enqueue(action);
        }

        public void WriteJson(string destFileName, object serializableObject)
        {            
            string ppJson = MiniJSON.Json.Serialize(serializableObject);
            File.WriteAllText(destFileName, ppJson);
        }

        public void WriteTrials(List<OrderedResultDict> dataDict, string[] headers, string fpath)
        {
            string[] csvRows = new string[dataDict.Count + 1];
            csvRows[0] = string.Join(",", headers.ToArray());
            object[] row = new object[headers.Length];

            for (int i = 1; i <= dataDict.Count; i++)
            {
                try
                {
                    dataDict[i - 1].Values.CopyTo(row, 0);
                    csvRows[i] = string.Join(",", row.Select(v => v.ToString()).ToArray());
                }
                catch (System.NullReferenceException)
                {
                    
                }              
                
            }

            File.WriteAllLines(fpath, csvRows);
        }

        public void WriteMovementData(List<float[]> data, string fpath)
        {
            string[] csvRows = new string[data.Count + 1];
            csvRows[0] = string.Join(",", Tracker.header);
            for (int i = 1; i <= data.Count; i++)
                csvRows[i] = string.Join(",", data[i-1].Select(f => f.ToString("0.####")).ToArray());

            File.WriteAllLines(fpath, csvRows);
        }


        public void ReadCSV(string fpath, System.Action<DataTable> callback)
        {
            // This code assumes the file is on disk, and the first row of the file
            // has the names of the columns on it. Returns null if not found

            DataTable data = null;
            try
            {
                data = CSVFile.CSV.LoadDataTable(fpath);
            }
            catch (FileNotFoundException)
            {

            }

            System.Action action = new System.Action(() => callback.Invoke(data));
            executeOnMainThreadQueue.Enqueue(action);
        }

        public void WriteCSV(DataTable data, string fpath)
        {
            var writer = new CSVFile.CSVWriter(fpath);
            writer.Write(data, true);
            writer.Dispose();
        }

        /// <summary>
        /// Any actions which are enqueued to run on Unity's main thread.
        /// </summary>
        void ManageActions()
        {
            while (executeOnMainThreadQueue.Count > 0)
            {
                executeOnMainThreadQueue.Dequeue().Invoke();
            }
        }

        /// <summary>
        /// Aborts the FileIOManager's thread.
        /// </summary>
        public void Quit()
        {
            t.Abort();
        }

    }

}