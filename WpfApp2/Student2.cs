using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp2
{
    class Student2:DependencyObject
    {
        public string Name2
        {
            get { return (string)GetValue(Name2Property); }
            set { SetValue(Name2Property, value); }
        }
        public static readonly DependencyProperty Name2Property = 
            DependencyProperty.Register("Name2", typeof(string), typeof(Student2));



        public string Name3
        {
            get { return (string)GetValue(Name3Property); }
            set { SetValue(Name3Property, value); }
        }

        // Using a DependencyProperty as the backing store for Name3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Name3Property =
            DependencyProperty.Register("Name3", typeof(string), typeof(Student2), new PropertyMetadata(""));


    }
}
