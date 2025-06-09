using System;
using System.IO;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Text;


public class ImageRecordModel
{
    private readonly string _passFolder;
    private readonly string _failFolder;
    private readonly string _csvFilePath;







    public ImageRecordModel(string passFolder, string failFolder, string csvFilePath)
    {
        _passFolder = passFolder;
        _failFolder = failFolder;
        _csvFilePath = csvFilePath;

        // 创建文件夹（如果不存在）
        if (!Directory.Exists(_passFolder))
            Directory.CreateDirectory(_passFolder);

        if (!Directory.Exists(_failFolder))
            Directory.CreateDirectory(_failFolder);

        // 创建 CSV 文件（如果不存在）
        
    }

    public void SaveImage(Bitmap snapImage, string SO, ObservableCollection<string> snapClassificationResults, string manualResult)
    {
        if (snapImage == null || snapClassificationResults == null || snapClassificationResults.Count == 0)
            throw new ArgumentException("Image or classification results are not available for saving.");

        // 确定目标文件夹路径
        string baseFolder = manualResult == "pass" ? _passFolder : _failFolder;
        string destFolder = Path.Combine(baseFolder, SO); // 在对应目录下创建 SO 文件夹
        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder); // 如果文件夹不存在则创建
        }

        // 定义图像文件名称和存储路径
        string imageName = $"{SO}_SnapImage_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string destPath = Path.Combine(destFolder, imageName);

        try
        {
            // 保存图像文件
            using (var snapImageCopy = new Bitmap(snapImage))
            {
                snapImageCopy.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                

            }

            // 定义 CSV 文件路径
            string csvFilePath = Path.Combine(destFolder, "ImageRecord.csv");

            // 如果 CSV 文件不存在，则创建并写入标题行
            if (!File.Exists(csvFilePath))
            {
                using (var writer = new StreamWriter(csvFilePath, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Timestamp,ImageName,ManualResult,AI Result,Probability");
                }
            }

            // 更新 CSV 文件
            using (var writer = new StreamWriter(csvFilePath, true, new UTF8Encoding(true)))
            {
                string timestamp = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
                string classificationResults = string.Join(", ", snapClassificationResults); // 合并分类结果
                writer.WriteLine($"{timestamp},{imageName},{manualResult},{classificationResults}");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error saving image or updating CSV: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Permission error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }




    public Dictionary<string, int> CountClasses(JObject yoloResults)
    {
        var classCounts = new Dictionary<string, int>();

        // 獲取 inference_result 陣列
        var results = yoloResults["inference_result"] as JArray;
        if (results == null) return classCounts;

        // 遍歷每個結果
        foreach (var result in results)
        {
            // 提取 label
            string label = result["label"].ToString();

            // 計算每個類別的數量
            if (classCounts.ContainsKey(label))
            {
                classCounts[label]++;
            }
            else
            {
                classCounts[label] = 1;
            }
        }

        return classCounts;
    }

    public Dictionary<string, List<float>> CountClassesWithScores(JObject yoloResults)
    {
        // 使用 Dictionary 來儲存每個類別對應的 score 列表
        var classCountsWithScores = new Dictionary<string, List<float>>();

        // 獲取 inference_result 陣列
        var results = yoloResults["inference_result"] as JArray;
        if (results == null) return classCountsWithScores;

        // 遍歷每個結果
        foreach (var result in results)
        {
            // 提取 label 和 score
            string label = result["label"].ToString();
            float score = (float)result["score"];

            // 檢查該類別是否已經存在於字典中
            if (classCountsWithScores.ContainsKey(label))
            {
                // 如果存在，將新的 score 加入列表
                classCountsWithScores[label].Add(score);
            }
            else
            {
                // 如果不存在，創建一個新的列表並添加 score
                classCountsWithScores[label] = new List<float> { score };
            }
        }

        return classCountsWithScores;
    }

    public (Dictionary<string, List<float>>, string, float) CountClassesWithScoresAndMaxScore(JObject yoloResults)
    {
        var classCountsWithScores = new Dictionary<string, List<float>>();
        string highestScoreLabel = string.Empty;
        float highestScore = float.MinValue;

        var results = yoloResults["inference_result"] as JArray;
        if (results == null) return (classCountsWithScores, highestScoreLabel, highestScore);

        foreach (var result in results)
        {
            string label = result["label"].ToString();
            float score = (float)result["score"];

            if (classCountsWithScores.ContainsKey(label))
            {
                classCountsWithScores[label].Add(score);
            }
            else
            {
                classCountsWithScores[label] = new List<float> { score };
            }

            if (score > highestScore)
            {
                highestScore = score;
                highestScoreLabel = label;
            }
        }

        return (classCountsWithScores, highestScoreLabel, highestScore);
    }
}
