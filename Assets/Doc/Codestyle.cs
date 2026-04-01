using System;
using System.Collections.Generic;
using UnityEngine;

namespace LH {
    [Serializable]
    public class Codestyle {
        private const string KEY = "key";

        [SerializeField] public int SerializeField1;
        [SerializeField] private int _serializeField2;

        private int _privateField;
        public int PublicProperty { get; private set; }
        internal string InternalProperty { get; set; }


        // Пустые строки между методами и логическими блоками
        // Методы именуются PascalCase, параметры camelCase
        private void Init(int value) {
            // Вложенные вызовы и инициализация через конструктор
            _privateField = value;
        }

        // Однострочные методы допускаются через =>
        public int GetValue() => _privateField;

        // Использование var для локальных переменных, если тип очевиден
        public void ExampleMethod1(string input) {
            string result = input.ToLowerInvariant();

            if (string.IsNullOrEmpty(result))
                return;

            // ...
        }

        public void ExampleMethod2(int value0, int value1, int value2) {
            // если параметры помещаются в строку, можно писать в строку
        }

        public void ExampleMethod3(
            int value0,
            int value1,
            int value2,
            string text0,
            string text1,
            string text2
        ) {
            // если параметры в строку смотрятся громоздко, вся конструкция разворачивается
            // и каждый параметр пишется на отдельной строке
        }


        // Пробелы вокруг операторов
        private bool IsValid(int value) => value > 0;


        // Вложенные классы — после основных методов
        public class NestedComparer : IComparer<int> {
            public int Compare(int x, int y) {
                return x - y;
            }
        }

        // допускаются краткие однострочные комментарии на русском для пояснения сложных мест
    }
}