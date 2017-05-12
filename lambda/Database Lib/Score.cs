using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DatabaseLib {
	[Table("scores")]
	public class Score {
		[Key]
		public uint g_id {
			get;
			set;
		}
		public uint p_id {
			get;
			set;
		}
		public uint value {
			get;
			set;
		}
	}
}
