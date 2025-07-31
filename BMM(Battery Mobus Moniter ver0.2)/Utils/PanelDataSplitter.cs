using System;
using System.Collections.Generic;

public static class PanelDataSplitter
{
    private enum Section
    {
        Main,
        Cell,
        Temp,
        Low,
        High
    }

    public static void SplitToSections(
        string rawText,
        out string[] main,
        out string[] cell,
        out string[] temp,
        out string[] low,
        out string[] high)
    {
        main = cell = temp = low = high = Array.Empty<string>();

        var lines = rawText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        List<string> mainList = new List<string>();
        List<string> cellList = new List<string>();
        List<string> tempList = new List<string>();
        List<string> lowList = new List<string>();
        List<string> highList = new List<string>();

        Section currentSection = Section.Main;

        foreach (var line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("--- Cell Voltages"))
            {
                currentSection = Section.Cell;
                continue;
            }
            else if (trimmed.StartsWith("--- Temperatures"))
            {
                currentSection = Section.Temp;
                continue;
            }
            else if (trimmed.StartsWith("Lo-Status:"))
            {
                currentSection = Section.Low;
                lowList.Add(trimmed);
                continue;
            }
            else if (trimmed.StartsWith("Hi-Status:"))
            {
                currentSection = Section.High;
                highList.Add(trimmed);
                continue;
            }
            else if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            switch (currentSection)
            {
                case Section.Main:
                    mainList.Add(trimmed);
                    break;
                case Section.Cell:
                    cellList.Add(trimmed);
                    break;
                case Section.Temp:
                    tempList.Add(trimmed);
                    break;
                case Section.Low:
                    lowList.Add(trimmed);
                    break;
                case Section.High:
                    highList.Add(trimmed);
                    break;
            }
        }

        main = mainList.ToArray();
        cell = cellList.ToArray();
        temp = tempList.ToArray();
        low = lowList.ToArray();
        high = highList.ToArray();
    }
}
