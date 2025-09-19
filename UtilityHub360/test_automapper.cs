using System;
using AutoMapper;

namespace UtilityHub360.Test
{
    public class TestAutoMapper
    {
        public void Test()
        {
            // Test if AutoMapper can be instantiated
            var config = new MapperConfiguration(cfg => { });
            var mapper = config.CreateMapper();
            Console.WriteLine("AutoMapper is working!");
        }
    }
}
