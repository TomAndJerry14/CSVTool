using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TableData;
using UnityEditor;
using UnityEngine;

public class CSVToClassConverter
{
    [MenuItem("CSVTool/生成默认随机表的CSV类")]
    public static void GenerateSingleClassFromCSV()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "DefaultNumberWeight.csv");
        if (File.Exists(path))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string classContent = ConvertCSVToClassString(path);
            File.WriteAllText(Path.Combine(Application.dataPath, "Scripts", "Config", $"{name}.cs"), classContent);
        }
        else
        {
            Debug.LogError($"不存在该路径:{path}");
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("CSVTool/GenerateClass")]
    public static void GenerateClassFromCSV()
    {
        string path = Application.streamingAssetsPath;
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.EndsWith(".csv"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string classContent = ConvertCSVToClassString(file);
                    File.WriteAllText(Path.Combine(Application.dataPath, "Scripts", "Config", $"{name}.cs"), classContent);
                }
            }
        }

        AssetDatabase.Refresh();
    }

    public static string ConvertCSVToClassString(string csvFilePath)
    {
        // 从文件路径中提取类名（不包含扩展名）
        string className = Path.GetFileNameWithoutExtension(csvFilePath);

        if (string.IsNullOrEmpty(className))
        {
            throw new ArgumentException("无法从提供的文件路径中获取类名。", nameof(csvFilePath));
        }

        // 读取CSV文件的所有行
        string[] lines = File.ReadAllLines(csvFilePath);
        if (lines.Length < 2)
        {
            throw new InvalidDataException($"CSV文件需要至少两行，第一行为字段名，第二行为字段类型。{csvFilePath}");
        }

        // 字段名和类型分别从CSV文件的第一行和第二行获取
        string[] fieldNames = lines[0].Split(',');
        string[] fieldTypes = lines[1].Split(',');

        // 确保字段名和类型的数量相同
        if (fieldNames.Length != fieldTypes.Length)
        {
            throw new InvalidDataException($"字段名和字段类型的数量必须相同。{csvFilePath}");
        }

        StringBuilder classBuilder = new StringBuilder();

        classBuilder.AppendLine("using System.Collections.Generic;\r\nusing System.IO;\r\nusing UnityEngine;\r\n\r\n");

        // 添加命名空间声明
        classBuilder.AppendLine("namespace TableData");
        classBuilder.AppendLine("{");

        // 类定义
        classBuilder.AppendLine($"    public class {className}");
        classBuilder.AppendLine("    {");

        // 为每一对字段名及其类型生成C#字段声明
        for (int i = 0; i < fieldNames.Length; i++)
        {
            string fieldName = fieldNames[i].Trim();
            string fieldType = fieldTypes[i].Trim();

            // 简单的验证以确保字段名和类型是有效的
            if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(fieldType))
            {
                throw new InvalidDataException($"字段名或类型不能为空:{csvFilePath}, fieldName:{fieldName}, fieldType:{fieldType}");
            }

            try
            {
                // 字段类型转换为C#字段类型
                fieldType = ConvertToCSharpType(fieldType);
            }
            catch (Exception e)
            {
                Debug.LogError($"错误路径:{csvFilePath}");
                throw e;
            }

            classBuilder.AppendLine($"        public {fieldType} {fieldName} {{ get; set; }}");
        }

        classBuilder.AppendLine("    }");


        classBuilder.AppendLine($"    public static class {className}Map\r\n    {{\r\n        static string path => Path.Combine(Application.streamingAssetsPath, \"{className}.csv\");\r\n        public static Dictionary<int, {className}> Map => map;\r\n        private static Dictionary<int, {className}> map = new Dictionary<int, {className}>();\r\n\r\n        static {className}Map()\r\n        {{\r\n            map = CSVToObjectConverter.SerializeCSVToObjectDictionary<int, {className}>(path);\r\n        }}\r\n    }}");

        // 结束命名空间声明
        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    // 将CSV字段类型转换为C#字段类型
    private static string ConvertToCSharpType(string fieldType)
    {
        // 根据CSV中提供的类型简单映射到C#类型
        // 这里可以根据需要扩展更多类型映射
        switch (fieldType.ToLower())
        {
            case "int":
                return "int";
            case "string":
                return "string";
            case "bool":
                return "bool";
            case "float":
                return "float";
            default:
                throw new InvalidDataException($"不支持的字段类型：{fieldType}");
        }
    }
}