namespace BlackJackCamera.Services
{
    /// <summary>
    /// Маппинг классов модели YOLOv8 на категории банковских услуг с бейджами
    /// </summary>
    public static class CategoryBadgeMapper
    {
        /// <summary>
        /// Типы бейджей для определения стиля
        /// </summary>
        public enum BadgeType
        {
            Primary,   // Основные услуги (сине-фиолетовый)
            Secondary, // Остальные услуги (голубой → бирюзовый)
            Discount,  // Скидки (красный → прозрачный)
            Special    // Специальные предложения (анимированные)
        }

        /// <summary>
        /// Класс для хранения бейджа с типом
        /// </summary>
        public class Badge
        {
            public string Text { get; set; }
            public BadgeType Type { get; set; }

            public Badge(string text, BadgeType type = BadgeType.Secondary)
            {
                Text = text;
                Type = type;
            }
        }

        /// <summary>
        /// Определение категорий с их бейджами
        /// </summary>
        private static readonly Dictionary<string, List<Badge>> _categoryBadges = new()
        {
            ["Купюры"] = new List<Badge>
            {
                new Badge("Заказ наличных", BadgeType.Primary),
                new Badge("Обмен валюты", BadgeType.Primary),
                new Badge("Платежи", BadgeType.Primary),
                new Badge("Совместные финансы", BadgeType.Secondary),
                new Badge("Сейфы", BadgeType.Secondary),
                new Badge("Электронные деньги", BadgeType.Secondary),
                new Badge("Кредит наличными", BadgeType.Secondary),
                new Badge("Кэшбэк от партнеров", BadgeType.Discount),
                new Badge("Сбережения", BadgeType.Secondary),
                new Badge("Банкоматы и отделения", BadgeType.Secondary)
            },

            ["Автомобиль"] = new List<Badge>
            {
                new Badge("Автокредит", BadgeType.Primary),
                new Badge("ОСАГО", BadgeType.Primary),
                new Badge("КАСКО", BadgeType.Primary),
                new Badge("Оплата штрафов ГАИ", BadgeType.Secondary),
                new Badge("Мои авто", BadgeType.Secondary),
                new Badge("Оплата транспорта", BadgeType.Secondary),
                new Badge("FIT SERVICE 5%", BadgeType.Discount),
                new Badge("Планета АВТО 10%", BadgeType.Discount)
            },

            ["Дом"] = new List<Badge>
            {
                new Badge("Счета ЖКУ", BadgeType.Primary),
                new Badge("Ипотека", BadgeType.Primary),
                new Badge("Страхование недвижемости", BadgeType.Primary),
                new Badge("Оплата интернета, телефонии, ТВ", BadgeType.Secondary),
                new Badge("Изменить адрес регистрации", BadgeType.Secondary),
                new Badge("Кредит под залог недвижемости", BadgeType.Secondary),
                new Badge("Домовой 10%", BadgeType.Discount),
                new Badge("Ужин дома до 25%", BadgeType.Discount)
            },

            ["Здание"] = new List<Badge>
            {
                new Badge("Кредиты бизнесу на покупку/строительство недвижимости", BadgeType.Primary),
                new Badge("Лизинг нежилых помещений", BadgeType.Primary),
                new Badge("Банковские гарантии под строительство", BadgeType.Primary),
                new Badge("Инвестиции в девелоперские проекты", BadgeType.Secondary)
            },

            ["Ноутбук"] = new List<Badge>
            {
                new Badge("Рассрочка", BadgeType.Primary),
                new Badge("Кредит", BadgeType.Primary),
                new Badge("Haier до 10%", BadgeType.Discount),
                new Badge("Альфа-Маркет до 10%", BadgeType.Discount)
            },

            ["Смартфон"] = new List<Badge>
            {
                new Badge("Мобильный банк", BadgeType.Primary),
                new Badge("Перевод по номеру телефона", BadgeType.Primary),
                new Badge("Подключить NFC-оплату", BadgeType.Primary),
                new Badge("Кредит на смартфон", BadgeType.Secondary),
                new Badge("Связаться с поддержкой", BadgeType.Secondary),
                new Badge("Haier до 10%", BadgeType.Discount),
                new Badge("Альфа-Маркет до 10%", BadgeType.Discount)
            },

            ["Телефон"] = new List<Badge>
            {
                new Badge("В рассрочку в Альфа-Маркете", BadgeType.Special),
                new Badge("Мобильный банк", BadgeType.Primary),
                new Badge("Перевод по номеру телефона", BadgeType.Primary),
                new Badge("Подключить NFC-оплату", BadgeType.Secondary),
                new Badge("Связаться с поддержкой", BadgeType.Secondary),
                new Badge("Альфа-Маркет до 10%", BadgeType.Discount)
            },

            ["БытоваяТехника"] = new List<Badge>
            {
                new Badge("Потребительский кредит на технику", BadgeType.Primary),
                new Badge("Рассрочка при покупке в магазинах-партнёрах", BadgeType.Primary),
                new Badge("Страхование бытовой техники", BadgeType.Primary),
                new Badge("Кэшбэк за покупки техники", BadgeType.Discount)
            },

            ["Магазин"] = new List<Badge>
            {
                new Badge("Расходы и доходы", BadgeType.Primary),
                new Badge("Эквайринг (терминалы, QR-оплаты)", BadgeType.Primary),
                new Badge("Интернет-эквайринг (оплата на сайте, подписки)", BadgeType.Primary),
                new Badge("Кредиты бизнесу на открытие торговых точек", BadgeType.Secondary),
                new Badge("Инкассация и обслуживание касс", BadgeType.Secondary),
                new Badge("POS-кредиты (покупка товаров в кредит на месте)", BadgeType.Secondary)
            },

            ["Документы"] = new List<Badge>
            {
                new Badge("Счета ЖКУ", BadgeType.Primary),
                new Badge("Мои договоры", BadgeType.Primary),
                new Badge("Подписанные документы", BadgeType.Primary),
                new Badge("Счета к оплате", BadgeType.Secondary),
                new Badge("Подписка Альфа-Смарт", BadgeType.Secondary),
                new Badge("Мои данные", BadgeType.Secondary),
                new Badge("Чеки за покупки", BadgeType.Secondary),
                new Badge("Услуги по выпуску и ведению ценных бумаг", BadgeType.Secondary)
            },

            ["Ручка"] = new List<Badge>
            {
                new Badge("Подписанные документы", BadgeType.Primary),
                new Badge("Электронная подпись", BadgeType.Primary),
                new Badge("Корпоративные сувениры", BadgeType.Secondary)
            },

            ["Тетрадь"] = new List<Badge>
            {
                new Badge("Финансовое планирование", BadgeType.Primary),
                new Badge("Отчёты по операциям", BadgeType.Primary),
                new Badge("Планировщики в мобильном приложении банка", BadgeType.Primary),
                new Badge("Бухгалтерские сервисы для бизнеса", BadgeType.Secondary),
                new Badge("Чеки за покупки", BadgeType.Secondary)
            },

            ["Человек"] = new List<Badge>
            {
                new Badge("Стать зарплатным клиентом", BadgeType.Primary),
                new Badge("Заказ дебетовой карты", BadgeType.Primary),
                new Badge("Кредиты и депозиты для физлиц", BadgeType.Primary),
                new Badge("Страхование жизни и здоровья", BadgeType.Secondary),
                new Badge("Пенсионные программы", BadgeType.Secondary),
                new Badge("Вход по биометрии", BadgeType.Secondary),
                new Badge("Перевод", BadgeType.Secondary),
                new Badge("Подарок на Альфа-Маркете до 15%", BadgeType.Discount)
            }
        };

        /// <summary>
        /// Маппинг классов модели на категории
        /// </summary>
        private static readonly Dictionary<int, string> _classToCategory = new()
        {
            // Купюры (наличные деньги, монеты)
            [124] = "Купюры", // Монета

            // Автомобиль и транспорт
            [90] = "Автомобиль", // Машина
            [302] = "Автомобиль", // Наземное транспортное средство
            [567] = "Автомобиль", // Транспортное средство
            [14] = "Автомобиль", // Автозапчасть
            [74] = "Автомобиль", // Автобус
            [343] = "Автомобиль", // Мотоцикл
            [559] = "Автомобиль", // Грузовик
            [569] = "Автомобиль", // Автомобильный номер
            [43] = "Автомобиль", // Велосипед
            [44] = "Автомобиль", // Велосипедный шлем
            [45] = "Автомобиль", // Велосипедное колесо
            [53] = "Автомобиль", // Лодка
            [551] = "Автомобиль", // Поезд
            [564] = "Автомобиль", // Одноколёсный велосипед

            // Дом / квартира
            [257] = "Дом", // Дом

            // Здание
            [70] = "Здание", // Здание
            [354] = "Здание", // Офисное здание
            [555] = "Здание", // Домик на дереве

            // Ноутбук / ПК
            [304] = "Ноутбук", // Ноутбук
            [128] = "Ноутбук", // Компьютерный монитор
            [127] = "Ноутбук", // Компьютерная клавиатура
            [129] = "Ноутбук", // Компьютерная мышь

            // Телефон (приоритет для новой категории с рассрочкой)
            [339] = "Телефон", // Мобильный телефон
            [526] = "Телефон", // Телефон

            // Смартфон (оставляем для совместимости)
            [135] = "Смартфон", // Проводной телефон

            // Бытовая техника
            [252] = "БытоваяТехника", // Бытовая техника
            [419] = "БытоваяТехника", // Холодильник
            [575] = "БытоваяТехника", // Стиральная машина
            [160] = "БытоваяТехника", // Посудомоечная машина
            [333] = "БытоваяТехника", // Микроволновая печь
            [361] = "БытоваяТехника", // Духовка

            // Магазин
            [131] = "Магазин", // Магазин

            // Документы
            [54] = "Документы", // Книга
            [368] = "Документы", // Резак для бумаги
            [425] = "Документы", // Папка-скоросшиватель

            // Ручка
            [376] = "Ручка", // Ручка

            // Тетрадь / Блокнот
            [377] = "Тетрадь", // Пенал

            // Человек
            [381] = "Человек", // Человек
            [322] = "Человек", // Мужчина
            [594] = "Человек", // Женщина
            [63] = "Человек", // Мальчик
            [216] = "Человек", // Девочка

            // Части тела человека
            [260] = "Человек", // Рука человека
            [261] = "Человек", // Борода человека
            [262] = "Человек", // Тело человека
            [263] = "Человек", // Ухо человека
            [264] = "Человек", // Глаз человека
            [265] = "Человек", // Лицо человека
            [266] = "Человек", // Ступня человека
            [267] = "Человек", // Волосы человека
            [268] = "Человек", // Кисть руки человека
            [269] = "Человек", // Голова человека
            [270] = "Человек", // Нога человека
            [271] = "Человек", // Рот человека
            [272] = "Человек" // Нос человека
        };

        /// <summary>
        /// Получает список бейджей для распознанных объектов
        /// </summary>
        /// <param name="detections">Список распознанных объектов</param>
        /// <returns>Список уникальных бейджей или null если нет подходящих категорий</returns>
        public static List<Badge>? GetBadgesForDetections(List<Detection> detections)
        {
            if (detections == null || detections.Count == 0)
                return null;

            // Собираем все категории из распознанных объектов
            var categories = new HashSet<string>();

            foreach (var detection in detections)
            {
                if (_classToCategory.TryGetValue(detection.ClassId, out var category))
                {
                    categories.Add(category);
                }
            }

            // Если нет категорий, возвращаем null
            if (categories.Count == 0)
                return null;

            // Собираем все бейджи из найденных категорий
            var badges = new List<Badge>();
            var seenTexts = new HashSet<string>();

            foreach (var category in categories)
            {
                if (_categoryBadges.TryGetValue(category, out var categoryBadges))
                {
                    foreach (var badge in categoryBadges)
                    {
                        // Избегаем дубликатов по тексту
                        if (!seenTexts.Contains(badge.Text))
                        {
                            badges.Add(badge);
                            seenTexts.Add(badge.Text);
                        }
                    }
                }
            }

            return badges.Count > 0 ? badges : null;
        }

        /// <summary>
        /// Проверяет, есть ли бейджи для данного класса
        /// </summary>
        /// <param name="classId">ID класса объекта</param>
        /// <returns>true если есть бейджи для данного класса</returns>
        public static bool HasBadgesForClass(int classId)
        {
            return _classToCategory.ContainsKey(classId);
        }
    }
}