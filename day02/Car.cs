using System.Windows.Media;

namespace WpfBikeShop
{
    public class Car
    {
        public double Speed { get; set; }
        public Color Color { get; set; }
        public Human Driver { get; set; }

        public Car()
        {

        }

        
    }

    public class Human
    {
        public string FirstName { get; set; }
        public bool HasDrivingLicense { get; set; }
    }
}