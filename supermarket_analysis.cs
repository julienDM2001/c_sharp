using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ScottPlot;

class Program
{
    static DataTable LoadCsvData(string filePath)
    {
        DataTable dt = new DataTable();
        using (StreamReader sr = new StreamReader(filePath))
        {
            string[] headers = sr.ReadLine().Split(',');
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }

            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(',');
                DataRow dr = dt.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }
                dt.Rows.Add(dr);
            }
        }
        return dt;
    }

    static void AnalyzeData(DataTable dt)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        // Total Sales
        Console.WriteLine($"Total Sales: {dt.Rows.Count}");

        // Total Revenue
        double totalRevenue = 0;
        foreach (DataRow row in dt.Rows)
        {
            if (double.TryParse(row["Total"].ToString(), System.Globalization.NumberStyles.Any, culture, out double totalValue))
            {
                totalRevenue += totalValue;
            }
            else
            {
                Console.WriteLine($"Unable to parse 'Total' value: {row["Total"].ToString()}");
            }
        }
        Console.WriteLine($"Total Revenue: {totalRevenue}");

        // Average Rating per Product Line
        DataTable distinctProducts = dt.DefaultView.ToTable(true, "Product line");
        foreach (DataRow product in distinctProducts.Rows)
        {
            string productName = product[0].ToString();
            DataRow[] selectedRows = dt.Select($"[Product line] = '{productName.Replace("'", "''")}'");
            double totalRating = 0;
            foreach (DataRow row in selectedRows)
            {
                if (double.TryParse(row["Rating"].ToString(), System.Globalization.NumberStyles.Any, culture, out double ratingValue))
                {
                    totalRating += ratingValue;
                }
                else
                {
                    Console.WriteLine($"Unable to parse 'Rating' for product {productName}: {row["Rating"].ToString()}");
                }
            }
            double averageRating = totalRating / selectedRows.Length;
            Console.WriteLine($"Average Rating for {productName}: {averageRating}");
        }
    }

    static void MostPopularProductLine(DataTable dt)
    {
        // Group data by Product Line and count the occurrences
        var productsCount = new Dictionary<string, int>();

        foreach (DataRow row in dt.Rows)
        {
            string productLine = row["Product line"].ToString();

            if (productsCount.ContainsKey(productLine))
                productsCount[productLine]++;
            else
                productsCount[productLine] = 1;
        }

        var mostPopularProduct = productsCount.OrderByDescending(p => p.Value).First();
        Console.WriteLine($"Most Popular Product Line: {mostPopularProduct.Key} with {mostPopularProduct.Value} sales.");
    }
    static void SalesTrendOverTime(DataTable dt)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        // Group data by Date and sum the total sales
        var salesByDate = new Dictionary<DateTime, double>();

        foreach (DataRow row in dt.Rows)
        {
            if (DateTime.TryParse(row["Date"].ToString(), culture, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                if (double.TryParse(row["Total"].ToString(), System.Globalization.NumberStyles.Any, culture, out double total))
                {
                    if (salesByDate.ContainsKey(date))
                        salesByDate[date] += total;
                    else
                        salesByDate[date] = total;
                }
            }
        }

        // Sorting the data by date
        var sortedData = salesByDate.OrderBy(e => e.Key).ToList();

        // Splitting the data into X (dates) and Y (totals) for plotting
        double[] xValues = sortedData.Select(kvp => (double)kvp.Key.ToOADate()).ToArray();
        double[] yValues = sortedData.Select(kvp => kvp.Value).ToArray();

        // Creating the plot
        var plt = new ScottPlot.Plot(800, 600);
        plt.AddScatter(xValues, yValues, lineWidth: 2);
        plt.Title("Sales Trend Over Time");
        plt.XLabel("Date");
        plt.YLabel("Total Sales");

        // Save the plot to a file
        plt.SaveFig("SalesTrend.png");

        Console.WriteLine("Sales Trend Over Time graph saved as SalesTrend.png");
    }


    static void Main()
    {
        string filePath = "supermarket.csv";
        DataTable dataTable = LoadCsvData(filePath);
        AnalyzeData(dataTable);
        SalesTrendOverTime(dataTable);
        MostPopularProductLine(dataTable);
    }
}
