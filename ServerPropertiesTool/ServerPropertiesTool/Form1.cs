using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using System.Threading;
using System.IO;

namespace ServerPropertiesTool
{
    public partial class Form1 : Form
    {
        private string m_excelPath = null;
        private string m_savePath = null;
        private string m_inGameJson = null;

        private bool m_threading = false;

        public Form1()
        {
            InitializeComponent();
        }

        void ExcelDataConvert()
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application excelApp
                    = new Microsoft.Office.Interop.Excel.Application();

                // 워크북 열기
                Workbook wb = excelApp.Workbooks.Open(m_excelPath);

                // 워크 시트 열기
                Worksheet gameTable = wb.Worksheets[1];
                Worksheet itemTable = wb.Worksheets[2];

                // 읽어올 데이터
                object[,] excelData = null;

                // Json 변환 -- ingameTable
                #region InGameTable Json Convert
                excelData = gameTable.UsedRange.Value;

                string jsonData = "{";
                // 행
                for (int i = 2; i < excelData.GetLength(0); i++)
                {
                    // id 가져오기
                    jsonData += "\"" + excelData[i , 1] + "\":";

                    // value가 있는지 체크
                    if (excelData[i , 4] == null)
                    {
                        jsonData += excelData[i , 2] + ",\n";
                    }
                    // 없으면 여러개
                    else
                    {
                        jsonData += "\"" + excelData[i , 4] + "\",\n";
                    }
                }
                
                #endregion

                #region Item Json Convert

                jsonData += "\"Items\":[";
                excelData = itemTable.UsedRange.Value;
                for (int i = 2; i < excelData.GetLength(0); i++)
                {
                    // id 가져오기
                    jsonData 
                        += "{ \"Id\":" + "\""+excelData[i,1] +"\","+ "\"Type\":" + excelData[i,2]+ "}";

                    if (i < excelData.GetLength(0) - 1)
                        jsonData += ",\n";
                }
                jsonData += "]}";
                label_status.Text = "Excel 파일 준비 완료";
                m_inGameJson = jsonData;
                #endregion
            }
            catch (Exception err)
            {
                label_status.Text = "ERROR! 파일 다시!";
                MessageBox.Show("ERROR " + err);
            }
            finally
            {
                m_threading = false;
            }
        }

        void JsonDataSave()
        {
            try
            {
                StreamWriter wr = new StreamWriter(m_savePath);
                wr.Write(m_inGameJson);
                wr.Close();
                label_status.Text = "JSON 저장 완료";
            }
            catch(Exception err)
            {
                label_status.Text = "JSON 저장 실패";
                MessageBox.Show("ERROR! " + err);
            }
            finally
            {
                m_threading = false;
            }
            
        }

        // 엑셀 파일 선택
        private void button_excelSelect_Click(object sender , EventArgs e)
        {
            if (m_threading == true)
                return;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "Excel Files";
            dlg.Filter = "Excel File (*.xlsx) | *.xlsx";
            dlg.Multiselect = false;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
                m_threading = true;
                m_excelPath = dlg.FileName;
                label_status.Text = "JSON 변환중 ...";
                //ExcelDataConvert();
                Thread t1 = new Thread(new ThreadStart(ExcelDataConvert));
                t1.Start();
            }
        }

        // Json 저장
        private void button_JsonConvert_Click(object sender , EventArgs e)
        {
            if (string.IsNullOrEmpty(m_inGameJson) || m_threading)
                return;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = "properties";
            dlg.Filter = "Server Properties (*.properties) | *.properties";
            
            if(dlg.ShowDialog() == DialogResult.OK)
            {
                m_savePath = dlg.FileName;

                label_status.Text = "JSON 저장중 ...";

                Thread t1 = new Thread(new ThreadStart(JsonDataSave));
                t1.Start();
            }
        }

        // help Button 도움말 
        private void button_helpButton_Click(object sender , EventArgs e)
        {
            MessageBox.Show("도움말 : 엑셀 파일 선택 후 , 저장을 눌러 저장하시면 됩니다.");
        }
    }
}
