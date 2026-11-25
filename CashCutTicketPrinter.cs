using SetSoftware_Enterprise.ENTITIES.CASHCUTS;
using SetSoftware_Enterprise.ENTITIES.CASHOUTFLOWS;
using SetSoftware_Enterprise.ENTITIES.STORES;
using System;
using System.Collections.Generic;
using System.Text;

namespace SetSoftware_Enterprise.UTILS.TICKETS
{
    public class CashCutTicketPrinter
    {   
        private string LogoImagePath;
        private string TotalInWords;
        
        //Store Info
        public string StoreName;
        public string StoreAddress;
        public string StoreAddress2;

        //CashCut Info
        private CashCut xCashCut;

        //CashOutFlows 
        public List<CashOutflows> LstCashOutflows;

        //Administrator Ticket Flag
        public bool IsAdminTicket;

        private const int TICKET_WIDTH = 42; // Characters for 80mm printer at 12 CPI
        public CashCutTicketPrinter(CashCut CashCutObj, Store xStore) 
        { 
            this.xCashCut = CashCutObj;
            this.StoreName = xStore.Name;
            this.StoreAddress = xStore.Street + " " + xStore.Number;
            this.StoreAddress2 = xStore.City + " " + xStore.State + " " + xStore.ZipCode.ToString();
        }

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
            // CashCut Title (Centered)
            ticketData.AddRange(ESC.ALIGN_CENTER);
            ticketData.AddRange(ESC.BOLD_ON);
            if (this.IsAdminTicket)
            {
                ticketData.AddRange(Encoding.ASCII.GetBytes("Corte de Caja/Administrador"));
            }
            else
            {
                ticketData.AddRange(Encoding.ASCII.GetBytes("Corte de Caja/Usuario"));
            }
            ticketData.AddRange(ESC.BOLD_OFF);
            ticketData.AddRange(ESC.FEED_LINE);
            // Separator line
            ticketData.AddRange(ESC.ALIGN_LEFT);
            ticketData.AddRange(Encoding.ASCII.GetBytes(new string('-', TICKET_WIDTH)));
            ticketData.AddRange(ESC.FEED_LINE);

            //CashCut Number
            ticketData.AddRange(Encoding.ASCII.GetBytes("*ID Corte:"+xCashCut.Id.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //User
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Usuario:" + xCashCut.UserName));
            ticketData.AddRange(ESC.FEED_LINE);

            //Amount System Calculated
            if (IsAdminTicket)
            {
                ticketData.AddRange(Encoding.ASCII.GetBytes("*Total Ventas:$" + xCashCut.AmountSystemCalculated.ToString()));                
            }
            else 
            { 
                ticketData.AddRange(Encoding.ASCII.GetBytes("*Total Ventas:$" + xCashCut.SpecialSymbolTicket.ToString()));
            }
            ticketData.AddRange(ESC.FEED_LINE);

            //Amount Count By User            
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Contado/Usuario:$" + xCashCut.AmountCountByUser.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Amount Sales With Card 
            if (IsAdminTicket)
            {
                ticketData.AddRange(Encoding.ASCII.GetBytes("*Pago con Tarjeta:$" + xCashCut.AmountSalesWithCard.ToString()));
                ticketData.AddRange(ESC.FEED_LINE);
            }

            //Amount Cash Outflows            
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Salidas de Efectivo:$" + xCashCut.AmountOnCashOutFlows.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Detail of Cash Outflows
            if (this.LstCashOutflows is not null && this.LstCashOutflows.Count > 0) 
            {
                foreach (var item in this.LstCashOutflows) {
                    ticketData.AddRange(Encoding.ASCII.GetBytes("-"+item.Description+"->"+item.Amount.ToString()));    
                    ticketData.AddRange(ESC.FEED_LINE);
                }
            }

            //Amount Cash in Box Initial
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Fondo de Caja Inicial:$" + xCashCut.OldAmountCashInBox.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Amount Cash in Box
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Nuevo Fondo de Caja:$" + xCashCut.AmountCashInBox.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Number of Active Sales            
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Ventas Activas:" + xCashCut.NumberOfActiveSales.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Number of Canceled Sales            
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Ventas Canceladas:" + xCashCut.NumberOfCanceledSales.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Total Amount of Canceled Sales            
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Total Ventas Canceladas:$" + xCashCut.TotalAmountCanceledSales.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            //Amount Cash in Hand
            ticketData.AddRange(Encoding.ASCII.GetBytes("*Bolsa/Corte:$" + xCashCut.AmountRetired.ToString()));
            ticketData.AddRange(ESC.FEED_LINE);

            // Footer
            ticketData.AddRange(ESC.FEED_LINE);            
            ticketData.AddRange(ESC.ALIGN_CENTER);
            ticketData.AddRange(Encoding.ASCII.GetBytes(((DateTime)xCashCut.FirstSaleTS).ToString("yyyy-MM-dd HH:mm:ss")));
            ticketData.AddRange(ESC.FEED_LINE);
            ticketData.AddRange(Encoding.ASCII.GetBytes("->"));
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

        /// Prints the ticket to a printer (Windows example using raw printer)        
        public void PrintToDevice(string printerName)
        {
            byte[] ticketData = GenerateTicket();

            RawPrinterHelperUtils.DoPrint(ticketData, printerName);
        }
    }
}