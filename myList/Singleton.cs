using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace myList
{
    class Singleton
    {
        private static readonly Singleton instance = new Singleton();

        private Singleton() { }

        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }

        private Todo m_todo;

        private string signal;

        private string sequence;

        public void set_todo(Todo current)
        {
            m_todo = current;
        }

        public Todo get_todo()
        {
            return m_todo;
        }

        public void set_signal(string sig)
        {
            signal = sig;
        }

        public string get_signal()
        {
            return signal;
        }
    }
}
