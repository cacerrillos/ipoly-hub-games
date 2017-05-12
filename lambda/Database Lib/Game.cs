using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DatabaseLib {
	[Table("games")]
	public class Game {
		[Key]
		public uint id {
			get;
			set;
		}
		[MaxLength(255)]
		public string host {
			get;
			set;
		}
		[MaxLength(255)]
		public string title {
			get;
			set;
		}
		[MaxLength(255)]
		public string location {
			get;
			set;
		}
		public uint score_type {
			get;
			set;
		}
		public int direction {
			get;
			set;
		}
	}
}
