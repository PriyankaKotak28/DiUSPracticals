using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiUSComputerStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to DiUS Computer Store!");
            Console.WriteLine("Here are the Available items in the store");
            try
            {
                var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "");
                var data = File.ReadAllText(mainpath.Replace("file:\\", "") + @"\ProductsDetails.json");
                var details = JsonConvert.DeserializeObject<IEnumerable<ProductsDetailsModel>>(data);
                Console.WriteLine(details.ToStringTable(new[] { "SKU", "Name", "Price" }, a => a.SKU, a => a.Name, a => (a.Currency + Convert.ToString(a.Price))));
                Console.WriteLine("Please add comma seperated SKUs from above available products. ex : mbp,ipd,atv,atv,etc..");
                var selectedItems = Console.ReadLine();
                var totalPrice = CalculateTotalAmount(selectedItems, details);
                Console.WriteLine("TotalAmount : $" + String.Format("{0:0.00}", totalPrice));
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                var errMessage = ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                    errMessage = ex.InnerException.Message;
                Console.WriteLine("Error from Main Method : " + errMessage);
            }
        }

        private static double CalculateTotalAmount(string selectedItems = "", IEnumerable<ProductsDetailsModel> availableItems = null)
        {
            var totalCount = 0.00;
            try
            {
                if (!string.IsNullOrEmpty(selectedItems))
                {
                    var itemsList = selectedItems.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (itemsList != null && itemsList.Any())
                    {
                        var newItemList = itemsList.ToList();
                        var noofTV = itemsList.Count(s => s.ToLower().Trim().Contains("atv"));
                        var noofIpd = itemsList.Count(s => s.ToLower().Trim().Contains("ipd"));
                        var noofMbp = itemsList.Count(s => s.ToLower().Trim().Contains("mbp"));
                        var isMbpSelected = itemsList.Any(s => s.ToLower().Trim().Contains("mbp"));
                        var isVgaSelected = itemsList.Any(s => s.ToLower().Trim().Contains("vga"));

                        if (availableItems != null && availableItems.Any())
                        {
                            //rule 1 : 3 for 2 deal
                            if (noofTV > 2)
                            {
                                var getProd = availableItems.FirstOrDefault(s => s.SKU.ToLower().Trim().Contains("atv"));
                                var getPrice = (getProd != null ? getProd.Price : 0);
                                totalCount += ((noofTV - 1) * getPrice);
                                newItemList.RemoveAll(s => s.ToLower().Trim().Contains("atv"));
                            }
                            //rule 2 : more than 4 ipad -> 50 rupees off on each ipad price
                            if (noofIpd > 4)
                            {
                                var getProd = availableItems.FirstOrDefault(s => s.SKU.ToLower().Trim().Contains("ipd"));
                                var getPrice = (getProd != null ? getProd.Price : 0);
                                totalCount += (noofIpd * (getPrice - 50.00));
                                newItemList.RemoveAll(s => s.ToLower().Trim().Contains("ipd"));
                            }
                            //rule 3 :  if adapter is selected and macbook is selected then only count price of macbook
                            if (isMbpSelected && isVgaSelected)
                            {
                                var getProd = availableItems.FirstOrDefault(s => s.SKU.ToLower().Trim().Contains("mbp"));
                                var getPrice = (getProd != null ? getProd.Price : 0);
                                totalCount += (noofMbp * (getPrice));
                                newItemList.RemoveAll(s => s.ToLower().Trim().Contains("mbp"));
                                newItemList.RemoveAll(s => s.ToLower().Trim().Contains("vga"));
                            }
                            //calculate price if not match with Any Rule
                            if(newItemList != null && newItemList.Any())
                            {
                                foreach (var item in newItemList)
                                {
                                    var prod = availableItems.FirstOrDefault(s => s.SKU.ToLower().Trim().Contains(item.Trim().ToLower()));
                                    totalCount += (prod != null ? prod.Price : 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errMessage = ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                    errMessage = ex.InnerException.Message;
                Console.WriteLine("Error from CalculateTotalAmount Method : " + errMessage);
            }
            return totalCount;
        }
    }
}
