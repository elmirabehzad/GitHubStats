using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitHubStats.Models
{
    public class CommitsInDay
    {
        public string Day { get; set; }
        public int Count { get; set; }
    }
}