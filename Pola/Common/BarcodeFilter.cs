using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace Pola.Common
{
    /// <summary>
    /// BarcodeFilter rejects barcodes that has been detected by mistake. It passess only barcodes that has been 
    /// scanned a number of times in a row.
    /// </summary>
    public class BarcodeFilter
    {
        #region Fields

        private int minPass = 10;
        private int failsThreshold = 3;
        private List<BarcodeItem> barcodes = new List<BarcodeItem>();
        private int fails;
        private string lastBarcode = null;

        #endregion

        #region Properties

        public int MinPass
        {
            get { return minPass; }
            set { minPass = value; }
        }

        public int FailsThreshold
        {
            get { return failsThreshold; }
            set { failsThreshold = value; }
        }

        #endregion

        #region Subclass

        private class BarcodeItem
        {
            public string Barcode;
            public int Value = 1;

            public BarcodeItem(string barcode)
            {
                this.Barcode = barcode;
            }
        }

        #endregion

        #region Methods

        public bool Update(string barcode)
        {
            if (barcode == null && fails < failsThreshold)
            {
                fails++;
                return false;
            }

            fails = 0;
            bool found = false;
            bool detected = false;

            for (int i = barcodes.Count - 1; i >= 0; i--)
            {
                if (barcodes[i].Barcode.Equals(barcode))
                {
                    barcodes[i].Value++;
                    found = true;
                    if (barcodes[i].Value >= minPass)
                    {
                        if (barcode != lastBarcode)
                        {
                            detected = true;
                            lastBarcode = barcode;
                        }
                        barcodes[i].Value = 1;
                    }
                }
                else if (barcodes[i].Value > 0)
                    barcodes[i].Value--;
                else
                    barcodes.RemoveAt(i);
            }
            if (!found && barcode != null)
                barcodes.Add(new BarcodeItem(barcode));

            return detected;
        }

        #endregion
    }
}
