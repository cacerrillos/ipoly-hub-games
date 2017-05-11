using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DatabaseLib {
	[Table("score_type")]
	public class ScoreType {
		[Key]
		public uint id {
			get;
			set;
		}
		[MaxLength(255)]
		public string text {
			get;
			set;
		}
	}
}
