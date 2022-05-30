using UnityEngine;
using System;

public static class ReadExcel
{
    public static BarData[] ReadCSV(TextAsset csvFile)
    {
        Debug.Log("In readcsv");
        string[] data = csvFile.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);

        //8 is the amount of columns in the csv
        //-1 is to ignore the first row, which presumably are column names
        int tableSize = data.Length / 8 - 1;
        int columnCount = 8;
        int dataSize = data.Length;
        int arrayIndex = 0;

        var barDataArray = new BarData[tableSize];

        for (int i = columnCount; i < dataSize; i++)
        {
            var tableSizeFraction = i % columnCount;

            switch (tableSizeFraction)
            {
                case 0:
                    if (arrayIndex >= tableSize)
                    {
                        return barDataArray;
                    }
                    barDataArray[arrayIndex] = new BarData();
                    barDataArray[arrayIndex].date = data[i];
                    break;
                case 1:
                    barDataArray[arrayIndex].time = data[i];
                    break;
                case 2:
                    barDataArray[arrayIndex].open = float.Parse(data[i]);
                    
                    break;
                case 3:
                    barDataArray[arrayIndex].high = float.Parse(data[i]);
                  
                    break;
                case 4:
                    barDataArray[arrayIndex].low = float.Parse(data[i]);
                   
                    break;
                case 5:
                    barDataArray[arrayIndex].close = float.Parse(data[i]);
                   
                    break;
                case 6:
                    barDataArray[arrayIndex].up = int.Parse(data[i]);
                    break;
                case 7:
                    barDataArray[arrayIndex].down = int.Parse(data[i]);

                    ++arrayIndex;

                    break;

                default:
                    break;
            }

        }

        return barDataArray;
    }
}
