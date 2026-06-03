using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;

    public class Book
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Author { get; set; }
        public required string Description { get; set; }
    }

