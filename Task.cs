using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerApp
{
    public class Task
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public int IsComplete { get; set; }
        public DateTime Deadline { get; set; }
    }
}

