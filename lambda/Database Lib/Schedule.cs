using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DatabaseLib {
	[Table("schedule")]
	public class Schedule {
		[Key]
		public uint p_id {
			get;
			set;
		}
		[Key]
		public uint block {
			get;
			set;
		}
		[Key]
		public uint g_id {
			get;
			set;
		}
	}
}
