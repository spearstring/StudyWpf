﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyDataTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var respository = new SampleRepository();
            var customers = respository.GetCustomers();

            Console.WriteLine(JsonConvert.SerializeObject(customers, Formatting.Indented));
            //Console.WriteLine(customers);
        }
    }
}
