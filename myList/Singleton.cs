﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace myList
{
    class Singleton
    {
        //private static readonly Singleton instance = new Singleton();

        private static Singleton instance;

        private Database m_db;

        public TodoManager ViewModel { get; set; }

        private SolidColorBrush font_color;

        private Singleton()
        {
            picture_count = 1;
            m_db = Database.Instance;
            ViewModel = new TodoManager();
            font_color = new SolidColorBrush(Colors.Black);
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                    instance = new Singleton();
                return instance;
            }
        }

        public SolidColorBrush get_color()
        {
            return font_color;
        }

        public void set_color(SolidColorBrush sc)
        {
            font_color = sc;
        }

        private int picture_count;

        public void set_picture_count(int count)
        {
            picture_count = count;
        }

        public string get_picture_count()
        {
            return picture_count.ToString();
        }

        public void add_picture_count()
        {
            picture_count++;
            m_db.set_picture_count(picture_count.ToString());
        }

        private Todo m_todo;

        private string signal;

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
