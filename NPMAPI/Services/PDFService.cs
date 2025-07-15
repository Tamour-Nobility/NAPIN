using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using EdiFabric.Core.Model.Edi.X12;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NPMAPI.Models;
using NPMAPI.Repositories;
using Org.BouncyCastle.Asn1.Tsp;
using static NPOI.HSSF.Util.HSSFColor;

namespace NPMAPI.Services
{
    public class PDFService : IPDFRepository
    {
        public byte[] GenerateItemizedPatientStatment(long patientAccount, long practiceCode, long userId,  string messageToPrint)
        {
            byte[] b = { };
            var statementsListResponse = GetStatmentsList(patientAccount, practiceCode );
            if (statementsListResponse.Status == "success")
            {
                var statementsList = statementsListResponse.Response;
                b = GeneratePDFStatment(statementsList, "TOTAL DUE" , messageToPrint);


            }
            return b;
        }

        public byte[] GenerateItemizedPsForDownload(StatmentDownloadRequestModel model, long userId)
        {
            byte[] b = { };
            string patAcc = model.PatientAccount.ToString();
            string exIds = model.ExcludedClaimsIds.Length > 0 ? String.Join(",", model.ExcludedClaimsIds) : null;
            long pracCode = model.PracticeCode;
            string message = model.Message;
            var statementsListResponse = GetStatmentsListForDownload(patAcc, exIds, pracCode);
            if (statementsListResponse.Status == "success")
            {
                var statementsList = statementsListResponse.Response;
                b = GeneratePDFStatmentfordownload(statementsList, "PATIENT RESPONSIBILITY" , message);
            }
            return b;
        }

        public byte[] GenerateRollingFordowloadForDownload(string prac_code, string duration)
        {

            byte[] b = { };
            long pr = Convert.ToInt64(prac_code);
            int du = Convert.ToInt32('-' + duration);
            var rollingListResponse = GetRollingListForDownload(pr, du);
            if (rollingListResponse.Status == "success")
            {
                if (rollingListResponse.Response.Count > 0)
                {
                    var statementsList = rollingListResponse.Response;
                    b = GeneratePDFforReport(statementsList, du);
                }
               
            }
            return b;
        }




        private byte[] GeneratePDFforReport(dynamic list , int du)
        {
            List<rollingReportforSP> statementsList = list;
            MemoryStream memStream = new MemoryStream();
            byte[] pdfBytes;
            int duration;
            DateTime startDate;
            DateTime endDate;


            int[] days = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };


            decimal sumof2023charges = 0;
            decimal sumof2023payments = 0;
            decimal sumof2023adjustment = 0;
            decimal sumof2023refunds = 0;

            decimal sumof2022charges = 0;
            decimal sumof2022payments = 0;
            decimal sumof2022adjustment = 0;
            decimal sumof2022refunds = 0;

            decimal sumof2021charges = 0;
            decimal sumof2021payments = 0;
            decimal sumof2021adjustment = 0;
            decimal sumof2021refunds = 0;



            decimal sumof2020charges = 0;
            decimal sumof2020payments = 0;
            decimal sumof2020adjustment = 0;
            decimal sumof2020refunds = 0;
            int dua = du / -12;

            if(dua >= 1)
            {
                
                if(dua ==1)
                {
                     startDate = DateTime.Now.AddYears(-1);
                     endDate = DateTime.Now;
                    foreach(var xc in statementsList)
                    {
                        if (xc.Year.Equals("2023"))
                        {
                            if(xc.DataType == "Charges")
                            {
                                sumof2023charges = xc.Amount + sumof2023charges;
                            }else if(xc.DataType == "Payments")
                            {
                                sumof2023payments = xc.Amount + sumof2023payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2023refunds= xc.Amount + sumof2023refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2023adjustment = xc.Amount + sumof2023adjustment;
                            }


                        }
                        else if(xc.Year.Equals("2022"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2022charges = xc.Amount + sumof2022charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2022payments = xc.Amount + sumof2022payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2022refunds = xc.Amount + sumof2022refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2022adjustment = xc.Amount + sumof2022adjustment;
                            }
                        }
                    }
                }else if(dua == 2)
                {
                     startDate = DateTime.Now.AddYears(-2);
                     endDate = DateTime.Now;
                    foreach (var xc in statementsList)
                    {
                        if (xc.Year.Equals("2023"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2023charges = xc.Amount + sumof2023charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2023payments = xc.Amount + sumof2023payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2023refunds = xc.Amount + sumof2023refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2023adjustment = xc.Amount + sumof2023adjustment;
                            }

                        }
                        else if (xc.Year.Equals("2022"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2022charges = xc.Amount + sumof2022charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2022payments = xc.Amount + sumof2022payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2022refunds = xc.Amount + sumof2022refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2022adjustment = xc.Amount + sumof2022adjustment;
                            }
                           
                        }else if (xc.Year.Equals("2021"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2021charges = xc.Amount + sumof2021charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2021payments = xc.Amount + sumof2021payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2021refunds = xc.Amount + sumof2021refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2021adjustment = xc.Amount + sumof2021adjustment;
                            }
                        }
                    }
                }
                else
                {
                     startDate = DateTime.Now.AddYears(-3);
                     endDate = DateTime.Now;
                    foreach (var xc in statementsList)
                    {
                        if (xc.Year.Equals("2023"))
                        {

                            if (xc.DataType == "Charges")
                            {
                                sumof2023charges = xc.Amount + sumof2023charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2023payments = xc.Amount + sumof2023payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2023refunds = xc.Amount + sumof2023refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2023adjustment = xc.Amount + sumof2023adjustment;
                            }

                        }
                        else if (xc.Year.Equals("2022"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2022charges = xc.Amount + sumof2022charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2022payments = xc.Amount + sumof2022payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2022refunds = xc.Amount + sumof2022refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2022adjustment = xc.Amount + sumof2022adjustment;
                            }
                        }
                        else if (xc.Year.Equals("2021"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2021charges = xc.Amount + sumof2021charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2021payments = xc.Amount + sumof2021payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2021refunds = xc.Amount + sumof2021refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2021adjustment = xc.Amount + sumof2021adjustment;
                            }
                        }
                        else if (xc.Year.Equals("2020"))
                        {
                            if (xc.DataType == "Charges")
                            {
                                sumof2020charges = xc.Amount + sumof2020charges;
                            }
                            else if (xc.DataType == "Payments")
                            {
                                sumof2020payments = xc.Amount + sumof2020payments;
                            }
                            else if (xc.DataType == "Refunds")
                            {
                                sumof2020refunds = xc.Amount + sumof2020refunds;
                            }
                            else if (xc.DataType == "Adjustments")
                            {
                                sumof2020adjustment = xc.Amount + sumof2020adjustment;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var xc in statementsList)
                {
                    if (xc.Year.Equals("2023"))
                    {

                        if (xc.DataType == "Charges")
                        {
                            sumof2023charges = xc.Amount + sumof2023charges;
                        }
                        else if (xc.DataType == "Payments")
                        {
                            sumof2023payments = xc.Amount + sumof2023payments;
                        }
                        else if (xc.DataType == "Refunds")
                        {
                            sumof2023refunds = xc.Amount + sumof2023refunds;
                        }
                        else if (xc.DataType == "Adjustments")
                        {
                            sumof2023adjustment = xc.Amount + sumof2023adjustment;
                        }

                    }
                }
                startDate = DateTime.Now;
                endDate = DateTime.Now;

            }

            
            try
            {
              

                // Create the PDF document
                Document document = new Document();

                // Set the output file path
               
                PdfWriter writer = PdfWriter.GetInstance(document, memStream);

              

                // Open the document
                document.Open();

                // Add the report title
                Font titleFont = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
                var baseFont1 = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Content/fonts/ARLRDBD.ttf"), "Identity-H", BaseFont.EMBEDDED);
                Font bold = new Font(baseFont1, 10);
                var baseFont2 = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Content/fonts/arial.ttf"), "Identity-H", BaseFont.EMBEDDED);
                Font regular = new Font(baseFont2, 10);
                //iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(HostingEnvironment.MapPath("~/Content/Images/Logo.jpg"));
                // logo.ScaleAbsolute(120f, 54f);
                //logo.SetAbsolutePosition(-540, -400);
                //document.Add(logo);


                Paragraph title = new Paragraph();
                title.Alignment = Element.ALIGN_CENTER;
            
                Font dateRangeFont5 = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
                Chunk sys = new Chunk("Nobility PM System", dateRangeFont5);

                title.Add(sys);
               
                title.Add(Chunk.SPACETABBING);
                title.Add(Chunk.SPACETABBING);
                Chunk name = new Chunk(statementsList[0].prac_name, titleFont);
              
                title.Add(name);
                title.Add(Chunk.SPACETABBING);
                title.Add(Chunk.SPACETABBING);
                var timeUtc = DateTime.UtcNow;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
                string dateTime = String.Format("{0:MMMM d,yyyy HH:mm}", easternTime);



                Font dateRangeFont3 = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);



                Chunk time = new Chunk(dateTime, dateRangeFont3);
           
                title.Add(time);
                document.Add(title);


                Font dateRangeFont1 = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
                Paragraph titled = new Paragraph("Summary of AR(CPA Dashboard)", dateRangeFont1);
                titled.Alignment = Element.ALIGN_CENTER;
   
                document.Add(titled);
                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);



                // Add the report date range
                Font dateRangeFont = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
                Paragraph dateRange = new Paragraph("Summary of AR(CPA Dashboard)", dateRangeFont);
                dateRange.Alignment = Element.ALIGN_CENTER;
                dateRange.SpacingAfter = 20f;
                document.Add(dateRange);

                // Add the report data



                // Add table headers
                PdfPTable table1 = new PdfPTable(8);
             

                table1.TotalWidth = 560f;
                table1.LockedWidth = true;
                table1.SetTotalWidth(new[] { 70f, 70f, 70f, 70f, 70f, 70f, 70f, 70f });


                table1.SpacingBefore = 10f;



                //Table Heading row
                
                List<string> clHeadings = new List<string> { "Month", "Charges", "Payments", "Adjustments ", "Refunds", "Transfer To Collection", "Current  A/R", "Days In A/R" };

                for(int j=0; j<1; j++)
                {
                    foreach (string col in clHeadings)
                    {
                        var cell7 = new PdfPCell(new Phrase(col, bold))
                        {
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            BackgroundColor = new BaseColor(166, 166, 166)
                        };
                        cell7.SetLeading(12, 0);
                        table1.AddCell(cell7);
                    }
                }
            
                document.Add(table1);
                for (int year = startDate.Year; year <= endDate.Year; year++)
                {
                    PdfPTable dataTable = new PdfPTable(8);
                 

                    dataTable.TotalWidth = 560f;
                    dataTable.LockedWidth = true;
                    dataTable.SetTotalWidth(new[] { 70f, 70f, 70f, 70f, 70f, 70f, 70f, 70f });


                
              


                    //Table Heading row
                 
                    int indexes;

                    if(DateTime.Now.Year == year)
                    {
                        indexes = Convert.ToInt32(DateTime.Now.Month) ;
                        indexes--;
                    }
                    else
                    {
                        indexes = 12;
                    }
                  
                    for (int month = 1; month <= indexes; month++)
                    {

                      
                     dataTable.AddCell(new PdfPCell(new Phrase(new DateTime(year, month, 1).ToString("MMMM"))));


                        int index1 = 0;
                        foreach (var cp in statementsList)
                        {


                            index1++;
                            if (cp.Year.Equals(year.ToString()))
                            {
                                if (cp.Month.Equals(month))
                                {
                                    if (cp.DataType == "Charges")
                                    {
                                        var v = cp.Amount.ToString().Split('.');

                                        var cell = new PdfPCell(new Phrase(cp.Amount.ToString("N", new CultureInfo("en-US")), regular))
                                        {

                                            HorizontalAlignment = Element.ALIGN_RIGHT,
                                            VerticalAlignment = Element.ALIGN_MIDDLE
                                        };
                                        cell.SetLeading(13, 0);
                                        dataTable.AddCell(cell);

                                        break;
                                    }


                                }

                            }

                            if (index1 == statementsList.Count)
                            {
                                dataTable.AddCell(new PdfPCell(new Phrase("")));
                            }
                        }
                        index1 = 0;
                        foreach (var cp in statementsList)
                        {
                            index1++;
                           
                            if (cp.Year.Equals(year.ToString()))
                            {
                                if (cp.Month.Equals(month))
                                {
                                    if (cp.DataType == "Payments")
                                    {

                                        var v = cp.Amount.ToString().Split('.');
                                        var cell = new PdfPCell(new Phrase(cp.Amount.ToString("N", new CultureInfo("en-US")), regular))
                                        {
                                         
                                            HorizontalAlignment = Element.ALIGN_RIGHT,
                                            VerticalAlignment = Element.ALIGN_MIDDLE
                                        };
                                        cell.SetLeading(13, 0);
                                        dataTable.AddCell(cell);

                                        break;
                                    }


                                }

                            }

                            if (index1 == statementsList.Count)
                            {
                                dataTable.AddCell(new PdfPCell(new Phrase("")));
                            }
                        }

                         index1 = 0;
                        foreach (var cp in statementsList)
                        {
                          
                            index1++;
                            if (cp.Year.Equals(year.ToString()))
                            {
                                if (cp.Month.Equals(month))
                                {
                                    if (cp.DataType == "Adjustments")
                                    {
                                        var v = cp.Amount.ToString().Split('.');
                                        var cell = new PdfPCell(new Phrase(cp.Amount.ToString("N", new CultureInfo("en-US")), regular))
                                        {
                                           
                                            HorizontalAlignment = Element.ALIGN_RIGHT,
                                            VerticalAlignment = Element.ALIGN_MIDDLE
                                        };
                                        cell.SetLeading(13, 0);
                                        dataTable.AddCell(cell);

                                        break;
                                    }


                                }

                            }


                            if (index1 == statementsList.Count)
                            {
                                dataTable.AddCell(new PdfPCell(new Phrase("")));
                            }
                        }
                        index1 = 0;
                        foreach (var cp in statementsList)
                        {
                           
                            index1++;
                            if (cp.Year.Equals(year.ToString()))
                            {
                                if (cp.Month.Equals(month))
                                {
                                    if (cp.DataType == "Refunds")
                                    {
                                        var v = cp.Amount.ToString().Split('.');
                                        var cell = new PdfPCell(new Phrase(cp.Amount.ToString("N",new CultureInfo("en-US")), regular))
                                        {

                                            HorizontalAlignment = Element.ALIGN_RIGHT,
                                            VerticalAlignment = Element.ALIGN_RIGHT
                                        };
                                        cell.SetLeading(13, 0);
                                        dataTable.AddCell(cell);

                                        break;
                                    }


                                }

                            }


                            if (index1 == statementsList.Count)
                            {
                                dataTable.AddCell(new PdfPCell(new Phrase("")));
                            }
                        }

                      
                          
                     

                        if (month == 13 || month == 14)
                        {
                            var cell1 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                                                                    ,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell1.SetLeading(13, 0);
                            dataTable.AddCell(cell1);
                        }
                        else
                        {
                            dataTable.AddCell(new PdfPCell(new Phrase("")));
                        }
                         index1 = 0;
                        foreach (var cp in statementsList)
                        {


                            index1++;
                            if (cp.Year.Equals(year.ToString()))
                            {
                                if (cp.Month.Equals(month))
                                {
                                    if (cp.DataType == "AR")
                                    {
                                        var v = cp.Amount.ToString().Split('.');

                                        var cell = new PdfPCell(new Phrase(cp.Amount.ToString("N", new CultureInfo("en-US")), regular))
                                        {

                                            HorizontalAlignment = Element.ALIGN_RIGHT,
                                            VerticalAlignment = Element.ALIGN_MIDDLE
                                        };
                                        cell.SetLeading(13, 0);
                                        dataTable.AddCell(cell);

                                        break;
                                    }


                                }

                            }

                            if (index1 == statementsList.Count)
                            {
                                dataTable.AddCell(new PdfPCell(new Phrase("")));
                            }
                        }

                        int index11 = 0;

                        DateTime DT2 = new DateTime(year, month, 01, 12, 00, 00);
                        DateTime daysinar = DT2.AddMonths(-2);
                        double amountLast3Moths = 0;
                        int daysLast3Moths = 0;
                      
                       
                        foreach (var cp in statementsList)
                        {
                            index11 ++;



                            if (daysinar.Year != DT2.Year)
                            {
                                object[] arr = new object[6];

                                if (DT2.Month == 3)
                                {
                                    arr[0] = 1;
                                    arr[1] = DT2.Year;
                                    arr[2] = 2;
                                    arr[3] = DT2.Year;
                                    arr[4] = 3;
                                    arr[5] = DT2.Year;
                                    daysLast3Moths = days[2] + days[1] + days[0];
                               
                                }
                                else if(DT2.Month == 2)
                                {
                                    arr[0] = 12;
                                    arr[1] = DT2.Year - 1;
                                    arr[2] = 1;
                                    arr[3] = DT2.Year;
                                    arr[4] = 2;
                                    arr[5] = DT2.Year;
                                
                                    daysLast3Moths = days[11] + days[1] + days[0];
                                    
                                }
                                else if(DT2.Month == 1)
                                {
                                    arr[0] = 11;
                                    arr[1] = DT2.Year -1;
                                    arr[2] = 12;
                                    arr[3] = DT2.Year - 1;
                                    arr[4] = 1;
                                    arr[5] = DT2.Year ;
                                    daysLast3Moths = days[10] + days[11] + days[0];
                                  
                                }

                               

                                if ((Convert.ToInt32(arr[1]) == Convert.ToInt32(cp.Year) && Convert.ToInt32(arr[0]) == cp.Month) || (Convert.ToInt32(arr[3]) == Convert.ToInt32(cp.Year) && Convert.ToInt32(arr[2]) == cp.Month) || (Convert.ToInt32(arr[5]) == Convert.ToInt32(cp.Year) && Convert.ToInt32(arr[4]) == cp.Month))
                                {
                                  
                                        if (cp.DataType == "Charges")
                                        {
                                            amountLast3Moths = Convert.ToDouble(cp.Amount) + amountLast3Moths;

                                        }


                                }


                            }
                            else
                            {
                                int[] myarray = { daysinar.Month, daysinar.Month + 1, daysinar.Month + 2 };
                                daysLast3Moths = days[daysinar.Month - 1] + days[(daysinar.Month+1) - 1] + days[(daysinar.Month + 2) - 1];

                                if (cp.Year.Equals(year.ToString()))
                                {
                                    if (myarray.Contains(cp.Month))
                                {
                                    if (cp.DataType == "Charges")
                                    {
                                             amountLast3Moths = Convert.ToDouble(cp.Amount)  + amountLast3Moths;
                                     
                                    }


                                }
                            }

                            }



                            if (index11 == statementsList.Count)
                            {
                                double daysinAR =0;
                                foreach (var cpo in statementsList)
                                {
                                    if (cpo.Year.Equals(DT2.Year.ToString()))
                                    {
                                        if (cpo.Month.Equals(DT2.Month))
                                        {
                                            if (cpo.DataType == "AR")
                                            {

                                                var valuelast = amountLast3Moths / daysLast3Moths;
                                                var v = Convert.ToDouble(cpo.Amount);
                                                if(valuelast > 0)
                                                {
                                                    daysinAR = v / valuelast;
                                                }
                                                else
                                                {
                                                    daysinAR = 0;
                                                }
                                                 

                                            
                                            }


                                        }

                                    }
                                }


                                var cell = new PdfPCell(new Phrase(daysinAR.ToString("0.0"), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_MIDDLE
                                };
                                cell.SetLeading(13, 0);
                                dataTable.AddCell(cell);


                            }
                        }









                    }


                    document.Add(dataTable);


                    PdfPTable dataTable1 = new PdfPTable(8);

                    dataTable1.TotalWidth = 560f;
                    dataTable1.LockedWidth = true;
                    dataTable1.SpacingAfter = 12.5f;
                    dataTable1.SetTotalWidth(new[] { 70f, 70f, 70f, 70f, 70f, 70f, 70f, 70f });


                    for(int i=0; i<=2; i++)
                    {
                        if (i == 0)
                        {
                            var cell1 = new PdfPCell(new Phrase(year.ToString() , regular))
                            {

                                HorizontalAlignment = Element.ALIGN_LEFT,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                                                                                                             ,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell1.SetLeading(13, 0);
                            dataTable1.AddCell(cell1);


                            if (year == 2023)
                            {
                                var cell = new PdfPCell(new Phrase(sumof2023charges.ToString("0.00"), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var cell3 = new PdfPCell(new Phrase(sumof2023payments.ToString("0.00"), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);

                                var cell2 = new PdfPCell(new Phrase(sumof2023adjustment.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var cell4 = new PdfPCell(new Phrase(sumof2023refunds.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);

                            }
                            else if (year == 2022)
                            {
                                var cell = new PdfPCell(new Phrase(sumof2022charges.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var cell3 = new PdfPCell(new Phrase(sumof2022payments.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);

                                var cell2 = new PdfPCell(new Phrase(sumof2022adjustment.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var cell4 = new PdfPCell(new Phrase(sumof2022refunds.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            else if (year == 2021)
                            {
                                var cell = new PdfPCell(new Phrase(sumof2021charges.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var cell3 = new PdfPCell(new Phrase(sumof2021payments.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);

                                var cell2 = new PdfPCell(new Phrase(sumof2021adjustment.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var cell4 = new PdfPCell(new Phrase(sumof2021refunds.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            else if (year == 2020)
                            {
                                var cell = new PdfPCell(new Phrase(sumof2020charges.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var cell3 = new PdfPCell(new Phrase(sumof2020payments.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);

                                var cell2 = new PdfPCell(new Phrase(sumof2020adjustment.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var cell4 = new PdfPCell(new Phrase(sumof2020refunds.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)

                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            else
                            {
                                dataTable1.AddCell(new PdfPCell(new Phrase("")));
                                dataTable1.AddCell(new PdfPCell(new Phrase("")));
                                dataTable1.AddCell(new PdfPCell(new Phrase("")));
                                dataTable1.AddCell(new PdfPCell(new Phrase("")));
                            }
                            var cell11 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell11.SetLeading(13, 0);
                            dataTable1.AddCell(cell11);
                            var cell12 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell12.SetLeading(13, 0);
                            dataTable1.AddCell(cell12);
                            var cell13 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell13.SetLeading(13, 0);
                            dataTable1.AddCell(cell13);

                        }
                        else if (i == 1)
                        {
                            var cell1 = new PdfPCell(new Phrase("Average", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_LEFT,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                                                                                                                                         ,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell1.SetLeading(13, 0);
                            dataTable1.AddCell(cell1);
                            if (year == 2023)
                            {
                                var v = sumof2023charges / indexes;

                                var cell = new PdfPCell(new Phrase(v.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var v1 = sumof2023payments / indexes;
                                var cell3 = new PdfPCell(new Phrase(v1.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);


                                var v2 = sumof2023adjustment / indexes;
                                var cell2 = new PdfPCell(new Phrase(v2.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var v3 = sumof2023refunds / indexes;
                                var cell4 = new PdfPCell(new Phrase(v3.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);

                            }
                            else if (year == 2022)
                            {
                                var v = sumof2022charges / indexes;

                                var cell = new PdfPCell(new Phrase(v.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var v1 = sumof2022payments / indexes;
                                var cell3 = new PdfPCell(new Phrase(v1.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);


                                var v2 = sumof2022adjustment / indexes;
                                var cell2 = new PdfPCell(new Phrase(v2.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var v3 = sumof2022refunds / indexes;
                                var cell4 = new PdfPCell(new Phrase(v3.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            else if (year == 2021)
                            {
                                var v = sumof2021charges / indexes;

                                var cell = new PdfPCell(new Phrase(v.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var v1 = sumof2021payments / 12;
                                var cell3 = new PdfPCell(new Phrase(v1.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);


                                var v2 = sumof2021adjustment / indexes;
                                var cell2 = new PdfPCell(new Phrase(v2.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var v3 = sumof2021refunds / indexes;
                                var cell4 = new PdfPCell(new Phrase(v3.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            else if (year == 2020)
                            {
                                var v = sumof2020charges / indexes;

                                var cell = new PdfPCell(new Phrase(v.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell.SetLeading(13, 0);
                                dataTable1.AddCell(cell);

                                var v1 = sumof2020payments / indexes;
                                var cell3 = new PdfPCell(new Phrase(v1.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);


                                var v2 = sumof2020adjustment / indexes;
                                var cell2 = new PdfPCell(new Phrase(v2.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);

                                var v3 = sumof2020refunds / indexes;
                                var cell4 = new PdfPCell(new Phrase(v3.ToString("N",new CultureInfo("en-US")), regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT,
                                    BackgroundColor = new BaseColor(166, 166, 166)
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            else
                            {

                            }

                            var cell11 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell11.SetLeading(13, 0);
                            dataTable1.AddCell(cell11);
                            var cell12 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell12.SetLeading(13, 0);
                            dataTable1.AddCell(cell12);
                            var cell13 = new PdfPCell(new Phrase(" ", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT,
                                BackgroundColor = new BaseColor(166, 166, 166)
                            };
                            cell13.SetLeading(13, 0);
                            dataTable1.AddCell(cell13);
                        }
                        if(i == 2)
                        {
                            dataTable1.AddCell(new PdfPCell(new Phrase("")));

                            var cell = new PdfPCell(new Phrase("Net Coll %", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT
                            };
                            cell.SetLeading(13, 0);
                            dataTable1.AddCell(cell);

                            if (year == 2023)
                            {

                                decimal charges = sumof2023charges;
                                decimal payments = sumof2023payments;
                                decimal adjust = sumof2023adjustment;

                                double value;
                                if (charges > 0 && adjust > 0)
                                {
                                    value = Convert.ToDouble(payments / (charges - adjust));
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell1 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell1.SetLeading(13, 0);
                                dataTable1.AddCell(cell1);
                            }
                            else if (year == 2022)
                            {
                                decimal charges = sumof2022charges;
                                decimal payments = sumof2022payments;
                                decimal adjust = sumof2022adjustment;

                                double value;
                                if (charges > 0 && adjust > 0)
                                {
                                    value = Convert.ToDouble(payments / (charges - adjust));
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell2 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);
                            }
                            else if (year == 2021)
                            {
                                decimal charges = sumof2021charges;
                                decimal payments = sumof2021payments;
                                decimal adjust = sumof2021adjustment;
                                double value;
                                if (charges > 0 && adjust > 0)
                                {
                                    value = Convert.ToDouble(payments / (charges - adjust));
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell3 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);
                            }
                            else if (year == 2020)
                            {
                                decimal charges = sumof2020charges;
                                decimal payments = sumof2020payments;
                                decimal adjust = sumof2020adjustment;
                                double value;
                                if (charges > 0 && adjust > 0)
                                {
                                    value = Convert.ToDouble(payments / (charges - adjust));
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                
                                var cell4 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }

                            var cel5 = new PdfPCell(new Phrase("Pay/Chg %", regular))
                            {

                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                VerticalAlignment = Element.ALIGN_RIGHT
                            };
                            cel5.SetLeading(13, 0);
                            dataTable1.AddCell(cel5);
                            if (year == 2023)
                            {

                                decimal charges = sumof2023charges;
                                decimal payments = sumof2023payments;
                                decimal adjust = sumof2023adjustment;
                                double value;
                                if (charges > 0)
                                {
                                    value = Convert.ToDouble(payments / charges);
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell1 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell1.SetLeading(13, 0);
                                dataTable1.AddCell(cell1);
                            }
                            else if (year == 2022)
                            {
                                decimal charges = sumof2022charges;
                                decimal payments = sumof2022payments;
                                decimal adjust = sumof2022adjustment;

                                double value;
                                if (charges > 0)
                                {
                                    value = Convert.ToDouble(payments / charges);
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell2 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell2.SetLeading(13, 0);
                                dataTable1.AddCell(cell2);
                            }
                            else if (year == 2021)
                            {
                                decimal charges = sumof2021charges;
                                decimal payments = sumof2021payments;
                                decimal adjust = sumof2021adjustment;
                                double value;
                                if (charges > 0)
                                {
                                    value = Convert.ToDouble(payments / charges);
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell3 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell3.SetLeading(13, 0);
                                dataTable1.AddCell(cell3);
                            }
                            else if (year == 2020)
                            {
                                decimal charges = sumof2020charges;
                                decimal payments = sumof2020payments;
                                decimal adjust = sumof2020adjustment;
                                double value;
                                if (charges > 0)
                                {
                                     value =Convert.ToDouble( payments / charges);
                                }
                                else
                                {
                                    value = 0.00;
                                }


                                double disValue = value * 100;
                                var cell4 = new PdfPCell(new Phrase(disValue.ToString("0.00") + " %", regular))
                                {

                                    HorizontalAlignment = Element.ALIGN_RIGHT,
                                    VerticalAlignment = Element.ALIGN_RIGHT
                                };
                                cell4.SetLeading(13, 0);
                                dataTable1.AddCell(cell4);
                            }
                            dataTable1.AddCell(new PdfPCell(new Phrase("")));
                            dataTable1.AddCell(new PdfPCell(new Phrase("")));
                            dataTable1.AddCell(new PdfPCell(new Phrase("")));


                        }
                        

                       
                    }
                   

                    document.Add(dataTable1);



               



                }



                
                // Close the document
                document.Close();
                pdfBytes = memStream.ToArray();
                return pdfBytes;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] GeneratePDFStatment(dynamic list, string sType , string messageToPrint)
        {
            MemoryStream memStream = new MemoryStream();
            byte[] pdfBytes;

            List<PatientStatementViewModelFromSpforprint> statementsList = list;
            statementsList = statementsList
            .GroupBy(s => s.claim_no) // Group by claim_no
            .SelectMany(group =>
            {
                // Separate charges and other rows
                var charges = group.Where(s => IdentifyByDesc(s.description) == "charges").ToList();
                var others = group.Where(s => IdentifyByDesc(s.description) != "charges").ToList();

                // Sort charges and attach related rows
                var orderedGroup = charges
                    .SelectMany(charge =>
                    {
                        var relatedPayments = others
                            .Where(o => o.paid_proc_code == charge.paid_proc_code)
                            .OrderBy(o => o.description); // Optional: Sort related payments
                        return new[] { charge }.Concat(relatedPayments);
                    })
                    .ToList();

                // Add rows with null paid_proc_code at the end
                var nullPaidProcCodeRows = others
                    .Where(o => o.paid_proc_code == null)
                    .OrderBy(o => o.description); // Optional: Sort these rows
                orderedGroup.AddRange(nullPaidProcCodeRows);

                return orderedGroup;
            })
            .ToList();

            List<decimal?> amounts = statementsList.Select(statement => statement.amount).ToList();

            //Data for top 2 right tables
            DateTime today = DateTime.Now;
            var message = messageToPrint; 
            string payDueDate = today.AddDays(15).ToString("MM/dd/yyyy") ?? "";
            var a = statementsList.FirstOrDefault().patAmtDue;
            var patResponsibily = a == 0 ? "$ 0.00" : String.Format("{0:C}", a);
            string patientAcc = statementsList.FirstOrDefault().patient_account;

            var Pat_Due = statementsList.Where(e => e.Pat_Amount != 0).FirstOrDefault();
            var Pat_DueResponsibily = Pat_Due.Pat_Amount;
            var Pat_DueResponsibilyy = Pat_DueResponsibily == 0 ? "$ 0.00" : String.Format("{0:C}", Pat_DueResponsibily);

            var Ins_Due = statementsList.Where(e => e.Ins_Amount != 0).FirstOrDefault();
            var Ins_DueResponsibily = Ins_Due.Ins_Amount;
            var Ins_DueResponsibilyy = Ins_DueResponsibily == 0 ? "$ 0.00" : String.Format("{0:C}", Ins_DueResponsibily);
            string statmentDate = DateTime.Now.ToString("MM/dd/yyyy");
            //Patient information
            string patientName = statementsList.FirstOrDefault().NAME;
            string patientAddress = statementsList.FirstOrDefault().PAT_ADDRESS ?? "";
            string patientAddress2 = "";
            string patientCity = statementsList.FirstOrDefault().CITY ?? "";
            string patientState = statementsList.FirstOrDefault().STATE ?? "";
            string patientZip = statementsList.FirstOrDefault().ZIP ?? "";
            string patientHomePhone = ""; //need to add
            //Practice information
            string practiceName = statementsList.FirstOrDefault().PRAC_NAME;
            string practiceAddress = statementsList.FirstOrDefault().PRAC_ADDRESS ?? "";
            string practiceAddress2 = "";
            string practiceCityStZip = statementsList.FirstOrDefault().ADDRESS ?? "";
            string practiceCity = practiceCityStZip == "" ? "" : practiceCityStZip.Substring(0, practiceCityStZip.IndexOf(","));
            string practiceState = practiceCityStZip == "" ? "" : practiceCityStZip.Substring(practiceCityStZip.IndexOf(",") + 1, 3);
            string practiceZip = practiceCityStZip == "" ? "" : practiceCityStZip.Substring(practiceCityStZip.Length - 5);
            string practicePhone = statementsList.FirstOrDefault().PRAC_PHONE ?? "";
            string practiceFax = "";


            // Pat_billing

            string Pat_billing_address = statementsList.FirstOrDefault().PAT_BILLING_ADDRESS;
            string Pat_billing_ZIP_State = statementsList.FirstOrDefault().PAT_BILLING_CITY_STATE_ZIP;
            string Pat_billing_Phone = statementsList.FirstOrDefault().BILLING_QUESTION_PHONE;


            try
            {
                //string pathToFiles = System.Web.Hosting.HostingEnvironment.MapPath("~/pdf/Statment" + System.DateTime.Now.Millisecond.ToString() + ".pdf");
                //System.IO.FileStream fs = new FileStream(pathToFiles, FileMode.Create);
                Rectangle rec = new Rectangle(0, 0, 612, 792);
                Document document = new Document(rec, 0, 0, 0, 0);
                PdfWriter writer = PdfWriter.GetInstance(document, memStream);

                document.Open();
                //Adding the logo
                // iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(HostingEnvironment.MapPath("~/Content/Images/Logo.jpg"));
                //   logo.ScaleAbsolute(238f, 114f);
                // logo.SetAbsolutePosition(18f, 641);
                // document.Add(logo);
                //Text under logo

                //   Block B //
                Font font = new Font(Font.FontFamily.TIMES_ROMAN, 12);
                ColumnText ct1 = new ColumnText(writer.DirectContent);
                ct1.SetSimpleColumn(new Rectangle(30f, 762f, 220f, 155f));
                //  ct1.SetSimpleColumn(new Rectangle(30f, 644f, 220f, 155f));
                // ct2.SetSimpleColumn(29f, 587f, 220f, 100f, 11f, 0);
                ct1.SetLeading(14, 0);
                ct1.SpaceCharRatio = 1f;
                ct1.AddText(new Paragraph(practiceName, font));
                ct1.AddText(Chunk.NEWLINE);
                ct1.AddText(new Paragraph(Pat_billing_address, font));
                ct1.AddText(Chunk.NEWLINE);
                ct1.AddText(new Paragraph(Pat_billing_ZIP_State, font));
                ct1.AddText(Chunk.NEWLINE);

                Paragraph p2 = new Paragraph();

                p2.Add(new Chunk("Billing Questions:", font));
                p2.Add(new Chunk(" " + FormattedPhone(Pat_billing_Phone) + "\n", font));

                ct1.AddText(p2);
                ct1.Go();
                //   "(###) ###-####"
                //Defining fonts for bold and regular texts
                var baseFont1 = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Content/fonts/ARLRDBD.ttf"), "Identity-H", BaseFont.EMBEDDED);
                Font bold = new Font(baseFont1, 10);
                var baseFont2 = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Content/fonts/arial.ttf"), "Identity-H", BaseFont.EMBEDDED);
                Font regular = new Font(baseFont2, 10);
                //Adding 1st table to top right 
                PdfPTable table1 = new PdfPTable(2);
                table1.SetTotalWidth(new[] { 142f, 142f });
                PdfPCell cellT111 = new PdfPCell(new Phrase("PAYMENT DUE DATE", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table1.AddCell(cellT111);
                PdfPCell cellT112 = new PdfPCell(new Phrase("PATIENT RESPONSIBILITY", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table1.AddCell(cellT112);
                PdfPCell cellT121 = new PdfPCell(new Phrase(payDueDate, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table1.AddCell(cellT121);
                PdfPCell cellT122 = new PdfPCell(new Phrase(Pat_DueResponsibilyy, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table1.AddCell(cellT122);

                table1.WriteSelectedRows(0, table1.Rows.Count, 297, 720, writer.DirectContentUnder);



                //Adding 5th table to top right 
                PdfPTable table9 = new PdfPTable(2);
                table9.SetTotalWidth(new[] { 142f, 142f });
                PdfPCell cellT91 = new PdfPCell(new Phrase("Check #", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 0,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table9.AddCell(cellT91);

                PdfPCell cellT97 = new PdfPCell(new Phrase("", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table9.AddCell(cellT97);

                table9.WriteSelectedRows(0, table9.Rows.Count, 297, 691, writer.DirectContentUnder);

                //Adding 2nd table to top right corner
                PdfPTable table2 = new PdfPTable(2);
                table2.SetTotalWidth(new[] { 142f, 142f });
                PdfPCell cellT211 = new PdfPCell(new Phrase("ACCOUNT NUMBER", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table2.AddCell(cellT211);
                PdfPCell cellT212 = new PdfPCell(new Phrase("STATEMENT DATE", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table2.AddCell(cellT212);
                PdfPCell cellT221 = new PdfPCell(new Phrase("" + patientAcc, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table2.AddCell(cellT221);
                PdfPCell cellT222 = new PdfPCell(new Phrase(statmentDate, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table2.AddCell(cellT222);
                table2.WriteSelectedRows(0, table1.Rows.Count, 297, 663, writer.DirectContentUnder);
                //Adding patient details

                // Block A //
                Font fbold = new Font(baseFont1, 8);
                Font fregular = new Font(baseFont2, 8);
                ColumnText ct2 = new ColumnText(writer.DirectContent);
                ct2.SetSimpleColumn(29f, 630f, 220f, 100f, 11f, 0);








                Paragraph p1 = new Paragraph();



                p1.Add(new Chunk(" " + patientName + "\n", font));


                p1.Add(new Chunk(" " + patientAddress + "\n", font));

                if (!string.IsNullOrEmpty(patientAddress2))
                {
                    p1.Add(new Chunk(" " + patientAddress2 + "\n", font));
                }

                //p1.Add(new Chunk(" " + patientAddress2 + "\n", fregular));

                p1.Add(new Chunk(" " + patientCity + ", " + patientState + " " + patientZip + "\n", font));

                //p1.Add(new Chunk(" " + patientZip + "\n", fregular));


                p1.Add(new Chunk(" " + FormattedPhone(patientHomePhone) + "\n", font));



                ct2.AddText(p1);






                ct2.Go();
               


                Paragraph p = new Paragraph();

                ColumnText ct3 = new ColumnText(writer.DirectContent);
                ct3.SetSimpleColumn(331f, 487f, 570f, 587f, 11f, 0);


                // table1.WriteSelectedRows(0, table1.Rows.Count, 297, 720, writer.DirectContentUnder);

                p.Add(new Chunk(" " + practiceName + "\n", fregular));


              
                p.Add(new Chunk(" " + practiceAddress + "\n", fregular));

                if (!string.IsNullOrEmpty(practiceAddress2))
                {
                    p.Add(new Chunk(" " + practiceAddress2 + "\n", fregular));
                }

                //p.Add(new Chunk(" " + practiceAddress2 + "\n", fregular));

                p.Add(new Chunk(" " + practiceCity + ", " + practiceState + ", " + practiceZip + "\n", fregular));

               
                //p.Add(new Chunk(" " + practiceZip + "\n", fregular));

               
                p.Add(new Chunk(" " + FormattedPhone(practicePhone) + "\n", fregular));

               
                p.Add(new Chunk(" " + practiceFax + "\n", fregular));

                ct3.AddText(p);
                ct3.Go();




                // Create a dotted line using PdfContentByte	
                PdfContentByte k = writer.DirectContent;
                k.SetLineDash(2, 2); // Set the line to be dotted	
                k.MoveTo(10, 490); // Set the starting point coordinates (x1, y1)	
                k.LineTo(600, 490); // Set the ending point coordinates (x2, y2)	
                k.Stroke();
                Paragraph p3 = new Paragraph();
                ColumnText ct4 = new ColumnText(writer.DirectContent);
                ct4.SetSimpleColumn(321f, 390f, 570f, 490f, 11f, 0); ;
                p3.Add(new Chunk("Please detach and return top portion with your payment", regular));
                ct4.AddText(p3);
                ct4.Go();
                PdfPTable table6 = new PdfPTable(1);
                table6.SetTotalWidth(new[] { 554F });
                PdfPCell cellT6 = new PdfPCell(new Phrase("Messages", bold))
                {
                    HorizontalAlignment = 0,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table6.AddCell(cellT6);
                PdfPCell cellT64 = new PdfPCell(new Phrase(message, regular))
                {
                    HorizontalAlignment = 10,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    FixedHeight = 27f
                };
                table6.AddCell(cellT64);
                table6.WriteSelectedRows(0, table6.Rows.Count, 29, 470, writer.DirectContentUnder);



                //Adding main table
                PdfPTable table3 = new PdfPTable(5)
                {
                    HeaderRows = 1
                };
                table3.SetTotalWidth(new[] { 75f, 212f, 77f, 95f, 95F });
                //Table Heading row
                List<string> clHeadings = new List<string> { "DATE", "DESCRIPTION", "CHARGES", "PAYMENTS & ADJUSTMENTS", "Due Amount" };
                foreach (string col in clHeadings)
                {
                    var cell = new PdfPCell(new Phrase(col, bold))
                    {
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = new BaseColor(166, 166, 166)
                    };
                    cell.SetLeading(12, 0);
                    table3.AddCell(cell);
                }
                //Table body rows
                var previousRow = new PatientStatementViewModelFromSpforprint();
                foreach (var row in statementsList)
                {
                    //var dos = row.date1 == null ? "" : row.date1?.ToString("MM/dd/yyyy");
                    //var cell = new PdfPCell(new Phrase("" + dos, regular))
                    //{
                    //    BorderWidthBottom = 0,
                    //    BorderWidthTop = 0,
                    //    HorizontalAlignment = 1,
                    //    VerticalAlignment = Element.ALIGN_MIDDLE
                    //};
                    //cell.SetLeading(13, 0);
                    //table3.AddCell(cell);
                    var charge = IdentifyByDesc(row.description) == "charges" ? FormattedAmount(row.amount) : "";
                    if (charge != "" && charge != null)
                    {
                        var dos = row.date1 == null ? "" : row.date1?.ToString("MM/dd/yyyy");



                        var cell = new PdfPCell(new Phrase("" + dos, bold))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };





                        cell.SetLeading(13, 0);
                        table3.AddCell(cell);
                    }
                    else
                    {
                        var dos = "";



                        var cell = new PdfPCell(new Phrase("" + dos, regular))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };





                        cell.SetLeading(13, 0);
                        table3.AddCell(cell);



                    }


                    string chks = IdentifyByDesc(row.description);

                    string desc =  row.description;
                    //var cell1 = new PdfPCell(new Phrase("" + desc, regular)) //.. this condition is commented and below is added by hamza akhlaq for adding cpt in PS
                    if (charge != "" && charge != null)
                    {
                        var cell1 = new PdfPCell(new Phrase("" + chks == "charges" ? row.proc_code + " - " + desc : desc, bold))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        cell1.SetLeading(13, 0);
                        table3.AddCell(cell1);
                    }
                    else
                    {
                        var cell1 = new PdfPCell(new Phrase("" + chks == "charges" ? row.proc_code + " - " + desc : desc, regular))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        cell1.SetLeading(13, 0);
                        table3.AddCell(cell1);

                    }
                  
             
                    string chk = IdentifyByDesc(row.description);
                    var charges = IdentifyByDesc(row.description) == "charges" ? FormattedAmount(row.amount) : "";
                    var paysnadjs = IdentifyByDesc(row.description) == "pna" ? FormattedAmount(row.amount) : "";
                    var patResponsibilityrowamoiunt = row.amtDue.ToString();
                    var cell2 = new PdfPCell(new Phrase("" + charges, bold))
                    {
                        BorderWidthBottom = 0,
                        BorderWidthTop = 0,
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    cell2.SetLeading(13, 0);
                    table3.AddCell(cell2);
                    var cell3 = new PdfPCell(new Phrase("" + paysnadjs, regular))
                    {
                        BorderWidthBottom = 0,
                        BorderWidthTop = 0,
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    cell3.SetLeading(13, 0);
                    table3.AddCell(cell3);
                    if (IdentifyByDesc(row.description) != "charges" ||
                      (previousRow != null && row.claim_no == previousRow.claim_no) ||
                      row.amtDue == 0)
                        {
                            patResponsibilityrowamoiunt = null;
                        }
                        else
                        {
                            patResponsibilityrowamoiunt = $" ${row.amtDue:0.00}";
                        }



                    var cell4 = new PdfPCell(new Phrase("" + patResponsibilityrowamoiunt, bold))
                    { 
                        BorderWidthBottom = 0,
                        BorderWidthTop = 0,
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    cell4.SetLeading(13, 0);
                    table3.AddCell(cell4);
                    previousRow = row;
                }
                //Tabl  e Footer row
                PdfPCell cellF1 = new PdfPCell(new Phrase("Total Patient Due", regular))
                {
                    Colspan = 4,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = 1,
                    MinimumHeight = 24
                };
                cellF1.SetLeading(13, 0);
                table3.AddCell(cellF1);
                PdfPCell cellF2 = new PdfPCell(new Phrase(Pat_DueResponsibilyy, regular))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = 1,
                    MinimumHeight = 24
                };
                cellF2.SetLeading(13, 0);
                table3.AddCell(cellF2);

                PdfPCell cellF11 = new PdfPCell(new Phrase("Total Insurance Due", regular))
                {
                    Colspan = 4,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = 1,
                    MinimumHeight = 24
                };
                cellF11.SetLeading(13, 0);
                table3.AddCell(cellF11);
                PdfPCell cellF22 = new PdfPCell(new Phrase(Ins_DueResponsibilyy, regular))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = 1,
                    MinimumHeight = 24
                };
                cellF22.SetLeading(13, 0);
                table3.AddCell(cellF22);

                float lastRowposition;

                if (table3.Rows.Count + 6 <= 18)
                {
                    lastRowposition = table3.WriteSelectedRows(0, 18, 28f, 420, writer.DirectContentUnder);
                }
                else
                {



                    table3.WriteSelectedRows(0, 17, 28f, 420, writer.DirectContentUnder);
                    //Add remaing rows on subsequent pages
                    int i = 17;
                    do
                    {
                        //if (i == 21 || i != table3.Rows.Count)
                        //{
                        //    PdfContentByte cb = writer.DirectContent;
                        //    cb.MoveTo(28f, 52);
                        //    cb.SetLineDash(0);
                        //    cb.LineTo(28 + 554, 52);
                        //    cb.Stroke();
                        //}
                        document.NewPage();
                        lastRowposition = table3.WriteSelectedRows(0, 1, 28f, 742, writer.DirectContentUnder);
                        lastRowposition = table3.WriteSelectedRows(i, i + 35, 28f, 714, writer.DirectContentUnder);
                        i += 35;
                    }
                    while (i <= table3.Rows.Count + 6);
                }

                PdfPTable table4 = new PdfPTable(2);
                table4.SetTotalWidth(new[] { 160f, 105f });
                //Table heading
                List<string> colHeadings = new List<string> { "ACCOUNT NUMBER", "STATEMENT DATE" };
                foreach (string col in colHeadings)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col))
                    {
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                    };
                    table4.AddCell(cell);
                }
                //Table body
                List<string> bodyCells = new List<string> { patientAcc, statmentDate, "PAYMENT DUE DATE", payDueDate, sType, Pat_DueResponsibilyy };
                foreach (string col in bodyCells)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col))
                    {
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                    };
                    table4.AddCell(col);
                }
                table4.WriteSelectedRows(0, table4.Rows.Count, 315, lastRowposition - 39, writer.DirectContent);

                PdfContentByte cb3 = writer.DirectContent;
                cb3.SaveState();
                cb3.BeginText();
                cb3.SetFontAndSize(baseFont1, 10);
                cb3.SetCharacterSpacing(0.3f);
                cb3.SetWordSpacing(0f);
                //cb3.SetTextMatrix(29f, lastRowposition - 39);
                //cb3.ShowText("MESSAGE:");
                cb3.SetTextMatrix(198f, lastRowposition - 100);
                cb3.ShowText("BILLING QUESTIONS:");
                cb3.SetTextMatrix(198f, lastRowposition - 110);
                cb3.ShowText(FormattedPhone(Pat_billing_Phone));
                cb3.EndText();
                cb3.RestoreState();

                // Close the document  
                document.Close();
                // Close the writer instance  
                writer.Close();
                // Write PDF to bytes
                pdfBytes = memStream.ToArray();
                return pdfBytes;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private byte[] GeneratePDFStatmentfordownload(dynamic list, string sType , string message )
        {
            MemoryStream memStream = new MemoryStream();
            byte[] pdfBytes;

            List<PatientStatementViewModelFromSp> statementsList = list;
                    statementsList = statementsList
            .GroupBy(s => s.claim_no) // Group by claim_no
            .SelectMany(group =>
            {
                // Separate charges and other rows
                var charges = group.Where(s => IdentifyByDesc(s.description) == "charges").ToList();
                var others = group.Where(s => IdentifyByDesc(s.description) != "charges").ToList();

                // Sort charges and attach related rows
                var orderedGroup = charges
                    .SelectMany(charge =>
                    {
                        var relatedPayments = others
                            .Where(o => o.paid_proc_code == charge.paid_proc_code)
                            .OrderBy(o => o.description); // Optional: Sort related payments
                        return new[] { charge }.Concat(relatedPayments);
                    })
                    .ToList();

                // Add rows with null paid_proc_code at the end
                var nullPaidProcCodeRows = others
                    .Where(o => o.paid_proc_code == null)
                    .OrderBy(o => o.description); // Optional: Sort these rows
                orderedGroup.AddRange(nullPaidProcCodeRows);

                return orderedGroup;
            })
            .ToList();





            //Data for top 2 right tables
            DateTime today = DateTime.Now;
            //var message = messageToPrint;
            string payDueDate = today.AddDays(15).ToString("MM/dd/yyyy") ?? "";
            var a = statementsList.FirstOrDefault().patAmtDue;
            var patResponsibily = a == 0 ? "$ 0.00" : String.Format("{0:C}", a);
            string patientAcc = statementsList.FirstOrDefault().patient_account;
            string statmentDate = DateTime.Now.ToString("MM/dd/yyyy");
            //Patient information
            string patientName = statementsList.FirstOrDefault().NAME;
            string patientAddress = statementsList.FirstOrDefault().PAT_ADDRESS ?? "";
            string patientAddress2 = "";
            var Pat_Due = statementsList.Where(e => e.Pat_Amount != 0).FirstOrDefault();
            var Pat_DueResponsibily = Pat_Due.Pat_Amount;
            var Pat_DueResponsibilyy = Pat_DueResponsibily == 0 ? "$ 0.00" : String.Format("{0:C}", Pat_DueResponsibily);

            string patientCity = statementsList.FirstOrDefault().CITY ?? "";
            string patientState = statementsList.FirstOrDefault().STATE ?? "";
            string patientZip = statementsList.FirstOrDefault().ZIP ?? "";
            string patientHomePhone = ""; //need to add
            //Practice information
            string practiceName = statementsList.FirstOrDefault().PRAC_NAME;
            string practiceAddress = statementsList.FirstOrDefault().PRAC_ADDRESS ?? "";
            string practiceAddress2 = "";
            string practiceCityStZip = statementsList.FirstOrDefault().ADDRESS ?? "";
            string practiceCity = practiceCityStZip == "" ? "" : practiceCityStZip.Substring(0, practiceCityStZip.IndexOf(","));
            string practiceState = practiceCityStZip == "" ? "" : practiceCityStZip.Substring(practiceCityStZip.IndexOf(",") + 1, 3);
            string practiceZip = practiceCityStZip == "" ? "" : practiceCityStZip.Substring(practiceCityStZip.Length - 5);
            string practicePhone = statementsList.FirstOrDefault().PRAC_PHONE ?? "";
            string practiceFax = "";


            // Pat_billing

            string Pat_billing_address = statementsList.FirstOrDefault().PAT_BILLING_ADDRESS;
            string Pat_billing_ZIP_State = statementsList.FirstOrDefault().PAT_BILLING_CITY_STATE_ZIP;
            string Pat_billing_Phone = statementsList.FirstOrDefault().BILLING_QUESTION_PHONE;


            try
            {
                //string pathToFiles = System.Web.Hosting.HostingEnvironment.MapPath("~/pdf/Statment" + System.DateTime.Now.Millisecond.ToString() + ".pdf");
                //System.IO.FileStream fs = new FileStream(pathToFiles, FileMode.Create);
                Rectangle rec = new Rectangle(0, 0, 612, 792);
                Document document = new Document(rec, 0, 0, 0, 0);
                PdfWriter writer = PdfWriter.GetInstance(document, memStream);

                document.Open();
                //Adding the logo
                // iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(HostingEnvironment.MapPath("~/Content/Images/Logo.jpg"));
                //   logo.ScaleAbsolute(238f, 114f);
                // logo.SetAbsolutePosition(18f, 641);
                // document.Add(logo);
                //Text under logo

                //   Block B //
                Font font = new Font(Font.FontFamily.TIMES_ROMAN, 12);
                ColumnText ct1 = new ColumnText(writer.DirectContent);
                ct1.SetSimpleColumn(new Rectangle(30f, 762f, 220f, 155f));
                ct1.SetLeading(14, 0);
                ct1.SpaceCharRatio = 1f;
                ct1.AddText(new Paragraph(practiceName, font));
                ct1.AddText(Chunk.NEWLINE);
                ct1.AddText(new Paragraph(Pat_billing_address, font));
                ct1.AddText(Chunk.NEWLINE);
                ct1.AddText(new Paragraph(Pat_billing_ZIP_State, font));
                ct1.AddText(Chunk.NEWLINE);

                Paragraph p2 = new Paragraph();

                p2.Add(new Chunk("Billing Questions:", font));
                p2.Add(new Chunk(" " + FormattedPhone(Pat_billing_Phone) + "\n", font));

                ct1.AddText(p2);
                ct1.Go();
                //   "(###) ###-####"
                //Defining fonts for bold and regular texts
                var baseFont1 = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Content/fonts/ARLRDBD.ttf"), "Identity-H", BaseFont.EMBEDDED);
                Font bold = new Font(baseFont1, 10);
                var baseFont2 = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Content/fonts/arial.ttf"), "Identity-H", BaseFont.EMBEDDED);
                Font regular = new Font(baseFont2, 10);
                //Adding 1st table to top right 
                PdfPTable table1 = new PdfPTable(2);
                table1.SetTotalWidth(new[] { 142f, 142f });
                PdfPCell cellT111 = new PdfPCell(new Phrase("PAYMENT DUE DATE", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table1.AddCell(cellT111);
                PdfPCell cellT112 = new PdfPCell(new Phrase("PATIENT RESPONSIBILITY", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table1.AddCell(cellT112);
                PdfPCell cellT121 = new PdfPCell(new Phrase(payDueDate, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table1.AddCell(cellT121);
                PdfPCell cellT122 = new PdfPCell(new Phrase(Pat_DueResponsibilyy, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table1.AddCell(cellT122);
                table1.WriteSelectedRows(0, table1.Rows.Count, 297, 720, writer.DirectContentUnder);




                //Adding 5th table to top right 	
                PdfPTable table9 = new PdfPTable(2);
                table9.SetTotalWidth(new[] { 142f, 142f });
                PdfPCell cellT91 = new PdfPCell(new Phrase("Check #", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 0,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table9.AddCell(cellT91);
                PdfPCell cellT97 = new PdfPCell(new Phrase("", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table9.AddCell(cellT97);
                table9.WriteSelectedRows(0, table9.Rows.Count, 297, 691, writer.DirectContentUnder);




                //Adding 2nd table to top right corner
                PdfPTable table2 = new PdfPTable(2);
                table2.SetTotalWidth(new[] { 142f, 142f });
                PdfPCell cellT211 = new PdfPCell(new Phrase("ACCOUNT NUMBER", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table2.AddCell(cellT211);
                PdfPCell cellT212 = new PdfPCell(new Phrase("STATEMENT DATE", bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table2.AddCell(cellT212);
                PdfPCell cellT221 = new PdfPCell(new Phrase("" + patientAcc, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table2.AddCell(cellT221);
                PdfPCell cellT222 = new PdfPCell(new Phrase(statmentDate, bold))
                {
                    HorizontalAlignment = 1,
                    VerticalAlignment = 1
                };
                table2.AddCell(cellT222);
                table2.WriteSelectedRows(0, table1.Rows.Count, 297, 663, writer.DirectContentUnder);
                //Adding patient details

                // Block A //
                Font fbold = new Font(baseFont1, 8);
                Font fregular = new Font(baseFont2, 8);
                ColumnText ct2 = new ColumnText(writer.DirectContent);
                //ct2.SetSimpleColumn(29f, 587f, 220f, 100f, 11f, 0);
                ct2.SetSimpleColumn(29f, 630f, 220f, 100f, 11f, 0);




                //Paragraph p1 = new Paragraph();

                //p1.Add(new Chunk("PATIENT'S NAME:", fbold));
                //p1.Add(new Chunk(" " + patientName + "\n", fregular));

                //p1.Add(new Chunk("STREET ADDRESS:", fbold));
                //p1.Add(new Chunk(" " + patientAddress + "\n", fregular));

                //p1.Add(new Chunk("ADDRESS 2:", fbold));
                //p1.Add(new Chunk(" " + patientAddress2 + "\n", fregular));


                //p1.Add(new Chunk("CITY, STATE:", fbold));
                //p1.Add(new Chunk(" " + patientCity + ", " + patientState + "\n", fregular));

                //p1.Add(new Chunk("ZIP CODE:", fbold));
                //p1.Add(new Chunk(" " + patientZip + "\n", fregular));

                //p1.Add(new Chunk("TELEPHONE:", fbold));
                //p1.Add(new Chunk(" " + FormattedPhone(patientHomePhone) + "\n", fregular));


                //ct2.AddText(p1);



                //<--Updated By Umer on 08/04/23 -->

                //<--Requetsed by Faraz -->

                Paragraph p1 = new Paragraph();



                p1.Add(new Chunk(" " + patientName + "\n", font));


                p1.Add(new Chunk(" " + patientAddress + "\n", font));

                if (!string.IsNullOrEmpty(patientAddress2))
                {
                    p1.Add(new Chunk(" " + patientAddress2 + "\n", font));
                }

                //p1.Add(new Chunk(" " + patientAddress2 + "\n", fregular));

                p1.Add(new Chunk(" " + patientCity + ", " + patientState + " " + patientZip + "\n", font));

                //p1.Add(new Chunk(" " + patientZip + "\n", fregular));


                p1.Add(new Chunk(" " + FormattedPhone(patientHomePhone) + "\n", font));


                ct2.AddText(p1);


                ct2.Go();
                Paragraph p = new Paragraph();

                ColumnText ct3 = new ColumnText(writer.DirectContent);
                ct3.SetSimpleColumn(331f, 487f, 570f, 587f, 11f, 0);


                p.Add(new Chunk(" Make Checks Payable To: " + "\n", regular));
                p.Add(new Chunk(" " + practiceName + "\n", fregular));



                p.Add(new Chunk(" " + practiceAddress + "\n", fregular));

                if (!string.IsNullOrEmpty(practiceAddress2))
                {
                    p.Add(new Chunk(" " + practiceAddress2 + "\n", fregular));
                }

                //p.Add(new Chunk(" " + practiceAddress2 + "\n", fregular));

                p.Add(new Chunk(" " + practiceCity + ", " + practiceState + ", " + practiceZip + "\n", fregular));


                //p.Add(new Chunk(" " + practiceZip + "\n", fregular));


                p.Add(new Chunk(" " + FormattedPhone(practicePhone) + "\n", fregular));


                p.Add(new Chunk(" " + practiceFax + "\n", fregular));

                ct3.AddText(p);
                ct3.Go();







                // Create a dotted line using PdfContentByte	
                PdfContentByte k = writer.DirectContent;
                k.SetLineDash(2, 2); // Set the line to be dotted	
                k.MoveTo(10, 490); // Set the starting point coordinates (x1, y1)	
                k.LineTo(600, 490); // Set the ending point coordinates (x2, y2)
                k.Stroke();
                Paragraph p3 = new Paragraph();
                ColumnText ct4 = new ColumnText(writer.DirectContent);
                ct4.SetSimpleColumn(321f, 390f, 570f, 490f, 11f, 0); ;
                p3.Add(new Chunk("Please detach and return top portion with your payment", regular));
                ct4.AddText(p3);
                ct4.Go();
                PdfPTable table6 = new PdfPTable(1);
                table6.SetTotalWidth(new[] { 554F });
                PdfPCell cellT6 = new PdfPCell(new Phrase("Messages", bold))
                {
                    HorizontalAlignment = 0,
                    VerticalAlignment = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166)
                };
                table6.AddCell(cellT6);
                PdfPCell cellT64 = new PdfPCell(new Phrase(message, regular))
                {
                    HorizontalAlignment = 10,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    FixedHeight = 27f
                };
                table6.AddCell(cellT64);
                table6.WriteSelectedRows(0, table6.Rows.Count, 29, 470, writer.DirectContentUnder);






                //Adding main table
                PdfPTable table3 = new PdfPTable(5)
                {
                    HeaderRows = 1
                };
                table3.SetTotalWidth(new[] { 75f, 212f, 77f, 95f, 95F });
                //Table Heading row
                List<string> clHeadings = new List<string> { "DATE", "DESCRIPTION", "CHARGES", "PAYMENTS & ADJUSTMENTS", "PATIENT RESPONSIBILITY" };
                foreach (string col in clHeadings)
                {
                    var cell = new PdfPCell(new Phrase(col, bold))
                    {
                        HorizontalAlignment = 1,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = new BaseColor(166, 166, 166)
                    };
                    cell.SetLeading(12, 0);
                    table3.AddCell(cell);
                }
                //Table body rows

                // Hamza Akhlaq(8/2/2023) Patient Statement Change
                long claimNoprivious = 0;
                long claimNocrunt = 0;


                var previousRow = new PatientStatementViewModelFromSp();
                foreach (var row in statementsList)
                {


                    string chk = IdentifyByDesc(row.description);
                    var patResponsibilyrowamounmt = "";
                    var claimNumber = row.claim_no;
                    var paysnadjs = IdentifyByDesc(row.description) == "pna" ? FormattedAmount(row.amount) : "";




                    var charges = IdentifyByDesc(row.description) == "charges" ? FormattedAmount(row.amount) : "";
                    if (charges != "" && charges != null)
                    {
                        var dos = row.date1 == null ? "" : row.date1?.ToString("MM/dd/yyyy");



                        var cell = new PdfPCell(new Phrase("" + dos, bold))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };





                        cell.SetLeading(13, 0);
                        table3.AddCell(cell);
                    }
                    else
                    {
                        var dos = "";



                        var cell = new PdfPCell(new Phrase("" + dos, regular))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };





                        cell.SetLeading(13, 0);
                        table3.AddCell(cell);



                    }


                    if (charges != "" && charges != null)
                    {
                        //string desc = row.description.Length > 41 ? row.description.Substring(0, 41) : row.description; // commented by Pir Ubaid to show whole discription.
                        string desc = row.description;
                        // var cell1 = new PdfPCell(new Phrase("" + desc, bold)) //..this line is commented and below is added by hamza akhlaq for cpt addition in PS
                        var cell1 = new PdfPCell(new Phrase("" + row.proc_code + " - " + desc, bold))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            NoWrap = false  // Set NoWrap to false to allow text wrapping
                        };
                        cell1.SetLeading(13, 0);
                        table3.AddCell(cell1);
                    }
                    else
                    {

                        //string desc = row.description.Length > 41 ? row.description.Substring(0, 41) : row.description; // commented by Pir Ubaid to show whole discription.
                        string desc = row.description;
                        var cell1 = new PdfPCell(new Phrase("" + desc, regular))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        cell1.SetLeading(13, 0);
                        table3.AddCell(cell1);
                    }
                    if (IdentifyByDesc(row.description) != "charges" || (previousRow != null && row.claim_no == previousRow.claim_no) || row.amtDue == 0)
                    {
                        patResponsibilyrowamounmt = "";
                    }
                    else
                    {
                        patResponsibilyrowamounmt = string.Format("${0:0.00}", row.amtDue);
                    }
                    //if (charges != "" && charges != null)
                    //{
                    //    //var result = FormattedAmount(row.amtDue);
                    //    //var patResponsibilyrowamounmts = string.Format("{0:0.00}", row.amount);
                    //    patResponsibilyrowamounmt = string.Format("{0:0.00}", row.amtDue);
                    //    //commented by pir ubaid(PATIENT RESPONSIBILITY was setting empty by this method)
                    //    //if (claimNoprivious == claimNumber)
                    //    //{
                    //    //    patResponsibilyrowamounmt = "";
                    //    //}
                    //}
                    //string chk = IdentifyByDesc(row.description);
                    //var charges = IdentifyByDesc(row.description) == "charges" ? FormattedAmount(row.amount) : "";
                    //var paysnadjs = IdentifyByDesc(row.description) == "pna" ? FormattedAmount(row.amount) : "";
                    //var patResponsibilyrowamounmt = row.amount;
                    var cell2 = new PdfPCell(new Phrase("" + charges, bold))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        cell2.SetLeading(13, 0);
                        table3.AddCell(cell2);
                        var cell3 = new PdfPCell(new Phrase("" + paysnadjs, regular))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        cell3.SetLeading(13, 0);
                        table3.AddCell(cell3);
                        var cell4 = new PdfPCell(new Phrase("" + patResponsibilyrowamounmt, bold))
                        {
                            BorderWidthBottom = 0,
                            BorderWidthTop = 0,
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        cell4.SetLeading(13, 0);
                        table3.AddCell(cell4);
                        claimNoprivious = claimNumber;
                    previousRow = row;
                }
                    //Table Footer row
                    PdfPCell cellF1 = new PdfPCell(new Phrase("Patient Responsibility", regular))
                    {
                        Colspan = 4,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = 1,
                        MinimumHeight = 24
                    };
                    cellF1.SetLeading(13, 0);
                    table3.AddCell(cellF1);
                    PdfPCell cellF2 = new PdfPCell(new Phrase(Pat_DueResponsibilyy, regular))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = 1,
                        MinimumHeight = 24
                    };
                    cellF2.SetLeading(13, 0);
                    table3.AddCell(cellF2);

                    float lastRowposition;

                    if (table3.Rows.Count + 6 <= 18)
                    {
                        lastRowposition = table3.WriteSelectedRows(0, 18, 28f, 420, writer.DirectContentUnder);
                    }
                    else
                    {



                        table3.WriteSelectedRows(0, 17, 28f, 420, writer.DirectContentUnder);
                        //Add remaing rows on subsequent pages
                        int i = 17;
                        do
                        {
                            //if (i == 21 || i != table3.Rows.Count)
                            //{
                            //    PdfContentByte cb = writer.DirectContent;
                            //    cb.MoveTo(28f, 52);
                            //    cb.SetLineDash(0);
                            //    cb.LineTo(28 + 554, 52);
                            //    cb.Stroke();
                            //}
                            document.NewPage();
                            lastRowposition = table3.WriteSelectedRows(0, 1, 28f, 742, writer.DirectContentUnder);
                            lastRowposition = table3.WriteSelectedRows(i, i + 35, 28f, 714, writer.DirectContentUnder);
                            i += 35;
                        }
                        while (i <= table3.Rows.Count + 6);
                    }

                    PdfContentByte dd = writer.DirectContent;

                    dd.SetLineDash(0);

                    dd.Stroke();

                    PdfPTable table4 = new PdfPTable(2);

                    table4.SetTotalWidth(new[] { 160f, 105f });
                    //Table heading
                    List<string> colHeadings = new List<string> { "ACCOUNT NUMBER", "STATEMENT DATE" };
                    foreach (string col in colHeadings)
                    {
                        PdfPCell cellx = new PdfPCell(new Phrase(col))
                        {

                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            BackgroundColor = new iTextSharp.text.BaseColor(166, 166, 166),

                        };
                        table4.AddCell(cellx);
                    }
                    //Table body
                    List<string> bodyCells = new List<string> { patientAcc, statmentDate, "PAYMENT DUE DATE", payDueDate, sType, Pat_DueResponsibilyy };
                    foreach (string col in bodyCells)
                    {
                        PdfPCell celly = new PdfPCell(new Phrase(col))
                        {
                            HorizontalAlignment = 1,
                            VerticalAlignment = Element.ALIGN_MIDDLE,

                        };
                        table4.AddCell(col);
                    }
                    table4.WriteSelectedRows(0, table4.Rows.Count, 315, lastRowposition - 39, writer.DirectContent);

                    PdfContentByte cb3 = writer.DirectContent;
                    cb3.SetLineDash(0);
                    cb3.SaveState();
                    cb3.BeginText();
                    cb3.SetFontAndSize(baseFont1, 10);
                    cb3.SetCharacterSpacing(0.3f);
                    cb3.SetWordSpacing(0f);
                    //cb3.SetTextMatrix(29f, lastRowposition - 39);
                    //cb3.ShowText("MESSAGE:");
                    cb3.SetTextMatrix(198f, lastRowposition - 100);
                    cb3.ShowText("BILLING QUESTIONS:");
                    cb3.SetTextMatrix(198f, lastRowposition - 110);
                    cb3.ShowText(FormattedPhone(Pat_billing_Phone));
                    cb3.EndText();
                    cb3.RestoreState();

                    // Close the document  
                    document.Close();
                    // Close the writer instance  
                    writer.Close();
                    // Write PDF to bytes
                    pdfBytes = memStream.ToArray();
                    return pdfBytes;
                }
            
            catch (Exception)
            {
                throw;
            }
        }


        private string IdentifyByDesc(string description)
        {
            string check;
            if (description.Substring(0, 3) == "Pri" || description.Substring(0, 3) == "Sec" || description.Substring(0, 3) == "Oth" || description == "Patient Payment" || description == "Copay" || description == "Write Off Adjustment")
            {
                check = "pna";
            }
            else
            {
                check = "charges";
            }
            return check;
        }

        
        private string makeit(string name)
        {
            var newName = name.Split()[0].ToUpper();
            var al = name.Substring(1,name.Length - 1).ToLower();
            return newName+al;
        }

        private string FormattedAmount(decimal? _amount)
        {
            return _amount == null ? "null" : string.Format("{0:C}", _amount.Value);
        }

        private string FormattedPhone(string _phone)

        {
            if (_phone == null || _phone == "")
            {
                return " ";
            }
            var newPhone = "(" + _phone[0] + _phone[1] + _phone[2] + ")" + " " + _phone[3] + _phone[4] + _phone[5] + "-" + _phone[6] + _phone[7] + _phone[8] + _phone[9];
            return newPhone;
        }

        private ResponseModel GetStatmentsList(long patientAccount, long practiceCode)
        {
            ResponseModel res = new ResponseModel();
            List<PatientStatementViewModelFromSpforprint> statementsList = new List<PatientStatementViewModelFromSpforprint>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetPatientWithClaimsForItemizedStatement(patientAccount.ToString(), null).ToList();
                    if (results.Count > 0)
                    {
                        string[] claimNosArray = results.Select(cn => cn.claim_no.ToString()).ToArray();
                        string claimNos = string.Join(",", claimNosArray);
                        statementsList.AddRange(
                                ctx.Database.SqlQuery<PatientStatementViewModelFromSpforprint>("SP_GENERATEPATIENTSTATEMENT @PATACCT,@PRAC,@CLMNO", parameters: new[] {
                                new SqlParameter("@PATACCT", patientAccount),
                                new SqlParameter("@PRAC", practiceCode),
                                new SqlParameter("@CLMNO", claimNos)
                                }).ToList());
                        res.Status = "success";
                        res.Response = statementsList;
                    }
                    else
                    {
                        res.Status = "failed";
                        res.Response = "No Claims for this patient Account";
                    }
                }
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private ResponseModel GetStatmentsListForDownload(string patientAccount, string excludedIds, long practiceCode)
        {
            ResponseModel res = new ResponseModel();
            List<PatientStatementViewModelFromSp> statementsList = new List<PatientStatementViewModelFromSp>();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var results = ctx.GetPatientWithClaimsForStatement(patientAccount, excludedIds).ToList();
                    if (results.Count > 0)
                    {
                        string[] claimNosArray = results.Select(cn => cn.claim_no.ToString()).ToArray();
                        string claimNos = string.Join(",", claimNosArray);
                        statementsList.AddRange(
                                ctx.Database.SqlQuery<PatientStatementViewModelFromSp>("SP_GENERATEPATIENTSTATEMENT @PATACCT,@PRAC,@CLMNO", parameters: new[] {
                                new SqlParameter("@PATACCT", patientAccount),
                                new SqlParameter("@PRAC", practiceCode),
                                new SqlParameter("@CLMNO", claimNos)
                                }).ToList());
                        res.Status = "success";
                        res.Response = statementsList;
                    }
                    else
                    {
                        res.Status = "failed";
                        res.Response = "No Claims for this patient Account";
                    }
                }
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private ResponseModel GetRollingListForDownload(long prac_code, long duration)
        {
            List<rollingReportforSP> statementsList = new List<rollingReportforSP>();
            ResponseModel res = new ResponseModel();
            try
            {
                using (var db = new NPMDBEntities())
                {

                   
                    statementsList.AddRange(
                            db.Database.SqlQuery<rollingReportforSP>("SP_rollingReport @PracticeCode,@duration", parameters: new[] {
                                new SqlParameter("@PracticeCode", prac_code),
                                new SqlParameter("@duration", duration),
                             
                            }).ToList());
                    res.Status = "success";
                    res.Response = statementsList;
                   
                }
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }




        #region message

        //<--Created By Umer Tariq -->

        //<--Requetsed by Faraz -->
        public ResponseModel AddMessage(PatientStatementMessages Model, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    var MessageID = Convert.ToInt64(ctx.SP_TableIdGenerator("Message_Id").FirstOrDefault().ToString());
                    ctx.Patient_Statement_Messages.Add(new Patient_Statement_Messages()
                    {
                        Message_ID = MessageID,
                        Messages = Model.Messages,
                        PracticeCode = Model.PracticeCode,
                        Created_By = userid,
                        Modified_By = null,
                        Modified_Date = null,
                        Deleted=false,
                        Created_Date = DateTime.Now,

                    });


                    if (ctx.SaveChanges() > 0)
                    {
                        responseModel.Status = "Success";
                    }
                    else
                    {
                        responseModel.Status = "Error";
                    };

                }

            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }

            return responseModel;

        }

        public ResponseModel EditMessage(PatientStatementMessages model, long userid)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                Patient_Statement_Messages _CL;
                using (var ctx = new NPMDBEntities())
                {
                    _CL = ctx.Patient_Statement_Messages.FirstOrDefault(u => u.Message_ID == model.Message_ID);
                    if (_CL != null)
                    {
                        _CL.Message_ID = model.Message_ID;
                        _CL.Messages = model.Messages;
                        _CL.Deleted = false;
                        _CL.Created_By = userid;
                        _CL.Modified_By = null;
                        _CL.Modified_Date = null;
                        _CL.Created_Date = DateTime.Now;


                        ctx.Entry(_CL).State = System.Data.Entity.EntityState.Modified;

                        if (ctx.SaveChanges() > 0)
                        {
                            responseModel.Status = "Success";
                        }


                        else
                        {
                            responseModel.Status = "Error";
                        }

                        return responseModel;
                    }
                }
            }





            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }


            return responseModel;
        }


        public ResponseModel DeleteMessage(long MessageID)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {

                using (var ctx = new NPMDBEntities())
                {

                    Models.Patient_Statement_Messages Obj = ctx.Patient_Statement_Messages.Find(MessageID);
                    if (Obj != null)
                    {
                        Obj.Deleted = true;
                        ctx.SaveChanges();
                    }

                    if (Obj != null)
                    {
                        objResponse.Status = "Success";
                        //objResponse.Response = FacilitiesList;
                    }
                    else
                    {
                        objResponse.Status = "Error";
                    }

                }
            }
            catch (Exception)
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }


        public ResponseModel GetAllMessages()
        {

            ResponseModel objResponse = new ResponseModel();
            List<Patient_Statement_Messages> objPatientList = null;
            using (var ctx = new NPMDBEntities())
            {
                objPatientList = ctx.Patient_Statement_Messages.Where(u => u.Deleted == null || u.Deleted == false).ToList();
            }

            if (objPatientList != null)
            {
                objResponse.Status = "Success";
                objResponse.Response = objPatientList;
            }
            else
            {
                objResponse.Status = "Error";
            }
            return objResponse;
        }



        #endregion

       










       








    }

}
