using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SetSoftware_Enterprise.UTILS.TICKETS
{    
    public class TicketItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
    
    /// Handles ticket creation and printing for 80mm thermal printers    
    public class SaleTicketPrinter
    {        

        private const int TICKET_WIDTH = 42; // Characters for 80mm printer at 12 CPI

        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string StoreAddress2 { get; set; }
        public List<TicketItem> Items { get; set; }
        public decimal Discount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string LogoImagePath { get; set; }
        public string TotalInWords { get; set; }
        public string SaleNumber { get; set; }
        public string SaleType { get; set; }

        public SaleTicketPrinter()
        {
            Items = new List<TicketItem>();
        }
        
        /// Generates the complete ticket data ready to send to printer        
        public byte[] GenerateTicket()
        {
            var ticketData = new List<byte>();

            // Initialize printer
            ticketData.AddRange(ESC.INIT);

            // Print logo if available
            if (!string.IsNullOrEmpty(LogoImagePath) && File.Exists(LogoImagePath))
            {
                ticketData.AddRange(ImageTicketUtils.PrintImage(LogoImagePath));
                ticketData.AddRange(ESC.FEED_LINE);
            }

            // Store name (centered, large)
            ticketData.AddRange(ESC.ALIGN_CENTER);
            ticketData.AddRange(ESC.DOUBLE_HEIGHT_WIDTH_ON);
            ticketData.AddRange(ESC.BOLD_ON);
            ticketData.AddRange(Encoding.ASCII.GetBytes(StoreName));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.NORMAL_SIZE);
            ticketData.AddRange(ESC.BOLD_OFF);

            // Store address (centered)
            ticketData.AddRange(Encoding.ASCII.GetBytes(StoreAddress));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(Encoding.ASCII.GetBytes(StoreAddress2));
            ticketData.AddRange(ESC.FEED_LINE);

            // Separator line
            ticketData.AddRange(ESC.ALIGN_LEFT);
            ticketData.AddRange(Encoding.ASCII.GetBytes(new string('-', TICKET_WIDTH)));
            ticketData.AddRange(ESC.FEED_LINE);

            // Items header
            ticketData.AddRange(ESC.BOLD_ON);
            string header = FormatItemLine("Articulo", "Cantidad", "Precio", "Total");
            ticketData.AddRange(Encoding.ASCII.GetBytes(header));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.BOLD_OFF);

            // Separator line
            ticketData.AddRange(Encoding.ASCII.GetBytes(new string('-', TICKET_WIDTH)));
            ticketData.AddRange(ESC.FEED_LINE);

            // Items
            foreach (var item in Items)
            {
                // Item name (full width if needed)
                if (item.Name.Length > 20)
                {
                    item.Name = item.Name.Substring(0, 19) + "*";
                }

                string itemLine = FormatItemLine(item.Name,
                                                 item.Quantity.ToString(),
                                                 item.UnitPrice.ToString("F2"),
                                                 item.Total.ToString("F2"));

                ticketData.AddRange(Encoding.ASCII.GetBytes(itemLine));
                
                ticketData.AddRange(ESC.FEED_LINE);
            }

            // Separator
            ticketData.AddRange(Encoding.ASCII.GetBytes(new string('-', TICKET_WIDTH)));
            ticketData.AddRange(ESC.FEED_LINE);

            // Subtotal            
            string subtotalLine = FormatTotalLine("SUBTOTAL:", this.Subtotal);
            ticketData.AddRange(Encoding.ASCII.GetBytes(subtotalLine));
            ticketData.AddRange(ESC.FEED_LINE);

            // Discount
            string discountLine = FormatTotalLine("DESCUENTO:", -this.Discount);
            ticketData.AddRange(Encoding.ASCII.GetBytes(discountLine));
            ticketData.AddRange(ESC.FEED_LINE);
            
            // Final total
            ticketData.AddRange(ESC.BOLD_ON);            
            decimal finalTotal = this.Total;
            string totalLine = FormatTotalLine("TOTAL:", finalTotal);

            ticketData.AddRange(Encoding.ASCII.GetBytes(totalLine));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.BOLD_OFF);
            ticketData.AddRange(ESC.FEED_LINE);

            // Total in words (centered)
            if (!string.IsNullOrEmpty(TotalInWords))
            {
                ticketData.AddRange(ESC.ALIGN_CENTER);
                ticketData.AddRange(Encoding.ASCII.GetBytes(TotalInWords));
                ticketData.AddRange(ESC.ALIGN_LEFT);
                ticketData.AddRange(ESC.FEED_LINE);
            }
            //Separator
            ticketData.AddRange(Encoding.ASCII.GetBytes(new string('-', TICKET_WIDTH)));

            //Sale Number            
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.BOLD_ON);
            ticketData.AddRange(Encoding.ASCII.GetBytes(FormatSaleNumber("#VENTA:" +this.SaleNumber)));            
            ticketData.AddRange(ESC.FEED_LINE);

            //SaleType
            ticketData.AddRange(Encoding.ASCII.GetBytes(FormatSaleNumber("PAGO:" + this.SaleType)));
            ticketData.AddRange(ESC.BOLD_OFF);
            ticketData.AddRange(ESC.FEED_LINE);

            //Separator
            ticketData.AddRange(Encoding.ASCII.GetBytes(new string('-', TICKET_WIDTH)));            

            // Footer
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.ALIGN_CENTER);
            ticketData.AddRange(Encoding.ASCII.GetBytes("Gracias por su Compra"));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(Encoding.ASCII.GetBytes(">>Set Software Enterprise>>"));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(ESC.FEED_LINE);     

            // Cut paper
            ticketData.AddRange(ESC.CUT_PAPER);

            return ticketData.ToArray();
        }
        
        /// Formats a line with item details        
        private string FormatItemLine(string name, string qty, string price, string total)
        {
            // Layout: Name(20) Qty(4) Price(7) Total(8)
            name = name.PadRight(20).Substring(0, 20);
            qty = qty.PadLeft(4).Substring(0, 4);
            price = price.PadLeft(7).Substring(0, 7);
            total = total.PadLeft(8).Substring(0, 8);

            return $"{name} {qty} {price} {total}";
        }        
        /// Formats a total line (subtotal, discount, total)        
        private string FormatTotalLine(string label, decimal amount)
        {
            string amountStr = amount.ToString("C2");
            int padding = TICKET_WIDTH - label.Length - amountStr.Length;
            return label + new string(' ', Math.Max(1, padding))+ amountStr;
        }
        /// Formats a #SALE  |          |
        private string FormatSaleNumber(String SaleId) 
        {
            int padding = TICKET_WIDTH - (SaleId.ToString().Length + "|       |".Length);
            return SaleId + new string(' ', Math.Max(1, padding)) + "|       |";
        }
        
        /// Prints the ticket to a printer (Windows example using raw printer)        
        public void PrintToDevice(string printerName)
        {
            byte[] ticketData = GenerateTicket();

            // Send to printer using RawPrinterHelper
            RawPrinterHelperUtils.DoPrint(ticketData, printerName);
        }
    }
}