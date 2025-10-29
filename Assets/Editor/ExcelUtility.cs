using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Excel;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System;

public class ExcelUtility
{

    private DataSet m_ResultSet;
    private FileStream m_Stream;
    public ExcelUtility(string excelFile)
    {
        m_Stream = File.Open(excelFile, FileMode.Open, FileAccess.Read);
        IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(m_Stream);
        m_ResultSet = mExcelReader.AsDataSet();
    }

    public List<T> ConvertToList<T>()
    {
        if (m_ResultSet.Tables.Count < 1)
            return null;
        DataTable mSheet = m_ResultSet.Tables[0];

        if (mSheet.Rows.Count < 1)
            return null;

        int rowCount = mSheet.Rows.Count;
        int colCount = mSheet.Columns.Count;

        List<T> list = new List<T>();

        for (int i = 1; i < rowCount; i++)
        {
            Type t = typeof(T);
            ConstructorInfo ct = t.GetConstructor(System.Type.EmptyTypes);
            T target = (T)ct.Invoke(null);
            for (int j = 0; j < colCount; j++)
            {
                string field = mSheet.Rows[0][j].ToString();
                object value = mSheet.Rows[i][j];
                SetTargetProperty(target, field, value);
            }

            list.Add(target);
        }

        return list;
    }



    public void ConvertToJson(string JsonPath, Encoding encoding)
    {
        if (m_ResultSet.Tables.Count < 1)
            return;

        DataTable mSheet = m_ResultSet.Tables[0];

        if (mSheet.Rows.Count < 1)
            return;

        int rowCount = mSheet.Rows.Count;
        int colCount = mSheet.Columns.Count;

        List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

        for (int i = 1; i < rowCount; i++)
        {
            Dictionary<string, object> row = new Dictionary<string, object>();
            for (int j = 0; j < colCount; j++)
            {
                string field = mSheet.Rows[0][j].ToString();
                if (field == "")
                {
                    break;
                }
                if (mSheet.Rows[i][j].ToString() == "")
                {
                    mSheet.Rows[i][j] = 0;
                }
                row[field] = mSheet.Rows[i][j].ToString();
            }

            table.Add(row);
        }

        string json = JsonConvert.SerializeObject(table, Newtonsoft.Json.Formatting.Indented);
        using (FileStream fileStream = new FileStream(JsonPath, FileMode.Create, FileAccess.Write))
        {
            using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
            {
                textWriter.Write(json);
            }
        }
    }


    public void ConvertToCSV(string CSVPath, Encoding encoding)
    {
        if (m_ResultSet.Tables.Count < 1)
            return;

        DataTable mSheet = m_ResultSet.Tables[0];

        if (mSheet.Rows.Count < 1)
            return;

        int rowCount = mSheet.Rows.Count;
        int colCount = mSheet.Columns.Count;

        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                stringBuilder.Append(mSheet.Rows[i][j] + ",");
            }
            stringBuilder.Append("\r\n");
        }

        using (FileStream fileStream = new FileStream(CSVPath, FileMode.Create, FileAccess.Write))
        {
            using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
            {
                textWriter.Write(stringBuilder.ToString());
            }
        }

    }

    public void ConvertToXml(string XmlFile)
    {
        if (m_ResultSet.Tables.Count < 1)
            return;

        DataTable mSheet = m_ResultSet.Tables[0];

        if (mSheet.Rows.Count < 1)
            return;

        int rowCount = mSheet.Rows.Count;
        int colCount = mSheet.Columns.Count;

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        stringBuilder.Append("\r\n");
        stringBuilder.Append("<Table>");
        stringBuilder.Append("\r\n");
        for (int i = 1; i < rowCount; i++)
        {
            stringBuilder.Append("  <Row>");
            stringBuilder.Append("\r\n");
            for (int j = 0; j < colCount; j++)
            {
                stringBuilder.Append("   <" + mSheet.Rows[0][j].ToString() + ">");
                stringBuilder.Append(mSheet.Rows[i][j].ToString());
                stringBuilder.Append("</" + mSheet.Rows[0][j].ToString() + ">");
                stringBuilder.Append("\r\n");
            }
            stringBuilder.Append("  </Row>");
            stringBuilder.Append("\r\n");
        }
        stringBuilder.Append("</Table>");
        using (FileStream fileStream = new FileStream(XmlFile, FileMode.Create, FileAccess.Write))
        {
            using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
            {
                textWriter.Write(stringBuilder.ToString());
            }
        }
    }

    private void SetTargetProperty(object target, string propertyName, object propertyValue)
    {
        Type mType = target.GetType();
        PropertyInfo[] mPropertys = mType.GetProperties();
        foreach (PropertyInfo property in mPropertys)
        {
            if (property.Name == propertyName)
            {
                property.SetValue(target, Convert.ChangeType(propertyValue, property.PropertyType), null);
            }
        }
    }

    public DataSet GetDataSet()
    {
        return m_ResultSet;
    }

    public DataTable GetDataSheet()
    {
        return m_ResultSet.Tables[0];
    }

    public void Save()
    {
        m_Stream.Close();
        
    }
}

