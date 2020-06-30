using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HAOC_ROBO_SULAMERICA.Helpers
{
    public class FileCreator
    {
        public string sPathLogTxt = @AppDomain.CurrentDomain.BaseDirectory + "\\Log\\" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";

        public FileCreator()
        {
            if (!isExists(sPathLogTxt))
            {
                WriteTxtLog("Arquivo de Log");
            }
        }


        public void FileCreationLog()
        {
            try
            {
                string sDateTime = DateTime.Now.ToString();

                StreamWriter oFileWriter = new StreamWriter(sPathLogTxt, true);

                oFileWriter.WriteLine("\n" + sDateTime);

                oFileWriter.Close();
            }
            catch (IOException ex)
            {
                WriteTxtLog("FileCreator.Error: " + ex);
            }

        }


        public void FileCreationLog(String str, bool success)
        {
            try
            {
                string sDateTime = DateTime.Now.ToString();

                StreamWriter oFileWriter = new StreamWriter(sPathLogTxt, true);

                if (success)
                {
                    oFileWriter.WriteLine("\n" + sDateTime + " SendEmailSuccess: " + str);
                }
                else
                {
                    oFileWriter.WriteLine("\n" + sDateTime + " SendEmailFailed: " + str);
                }


                oFileWriter.Close();

            }
            catch (IOException ex)
            {
                WriteTxtLog("FileCreator.Error: " + ex);
            }

        }



        public void WriteTxtLog(string text)
        {
            try
            {

                StreamWriter oFileWriter = new StreamWriter(this.sPathLogTxt, true);

                oFileWriter.WriteLine("\n" + text);

                oFileWriter.Close();

            }
            catch (IOException ex)
            {
                WriteTxtLog("FileCreator.Error: " + ex);
            }

        }

        public Boolean isExists(string TxtPath)
        {
            return File.Exists(TxtPath);
        }
    }
}
