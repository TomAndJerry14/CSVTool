using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using System.IO;

namespace TableData
{
    public class CSVToObjectConverter
    {
        public static Dictionary<TKey, TValue> ConvertCSVToObjectDictionary<TKey, TValue>(string csvFilePath) where TValue : new()
        {
            // 读取CSV文件的所有行
            string[] lines = File.ReadAllLines(csvFilePath);

            // 字段名和类型分别从CSV文件的第一行和第二行获取
            string[] fieldNames = lines[0].Split(new char[] { ',' }, StringSplitOptions.None);
            string[] fieldTypes = lines[1].Split(new char[] { ',' }, StringSplitOptions.None);

            // 跳过字段名和类型定义的前两行，从第三行开始读取数据
            Dictionary<TKey, TValue> objectDictionary = new Dictionary<TKey, TValue>();

            for (int i = 2; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                TValue instance = new TValue(); // 使用泛型参数T来创建对象实例

                for (int j = 0; j < fieldNames.Length; j++)
                {
                    // 使用反射为对象的属性赋值
                    PropertyInfo propertyInfo = typeof(TValue).GetProperty(fieldNames[j]);
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(instance, Convert.ChangeType(cells[j], propertyInfo.PropertyType), null);
                    }
                }

                var key = instance.GetType().GetProperty(fieldNames[0]).GetValue(instance);
                if (key is TKey)
                {
                    // CSV的第一列是唯一标识符
                    objectDictionary[(TKey)key] = instance;
                }
                else
                {
                    Debug.LogError($"{fieldNames[0]}的类型不是{typeof(TKey)}");
                }
            }

            return objectDictionary;
        }

        public static Dictionary<TKey, TValue> SerializeCSVToObjectDictionary<TKey, TValue>(string path) where TValue : new()
        {
            string pathToCSV = path; // CSV文件的路径
            return ConvertCSVToObjectDictionary<TKey, TValue>(pathToCSV);
        }
    }
}