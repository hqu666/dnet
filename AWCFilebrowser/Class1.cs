using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_tree_clock_web1
{
	class OrgFunc
	{
		//デバッグツール///////////////////////////////////////////////////////////その他//
		Boolean debug_now = true;
		public void MyLog(string msg)
		{
			if (debug_now)
			{
				Console.WriteLine(msg);
			}
		}

	}
}
