using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DatabaseLib {
	[Table("participants")]
	public class Participant {
		[Key]
		public uint id {
			get;
			set;
		}
		[MaxLength(255)]
		public string name {
			get;
			set;
		}
	}
}
