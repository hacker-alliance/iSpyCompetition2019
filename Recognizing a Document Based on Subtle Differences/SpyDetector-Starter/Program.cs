using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accusoft.ImagXpressSdk;
using Accusoft.FormFixSdk;
using Accusoft.FormDirectorSdk;
using System.Drawing;

namespace SpyDetector
{
    public class Program
    {

        static void Main(string[] args)
        {
            string SpyName;

            FormSetFile fdFormSetFile;

            fdFormSetFile = new FormSetFile(fd)
            {
                "//SecretPlans.frs"
            };
            fdFormSetFile.Read();

            SpyName = FormProcess.ProcessTheForm(ix, fdFormSetFile,);
        }
    }
}
