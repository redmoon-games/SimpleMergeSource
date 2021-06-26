# Именование полей
- Имя пространства (namespace) с большой буквы
- Имена классов большими буквами
- Имена Типов делегатов, экземпляров делегатов ?
- Имена событий (Action) начинать с приставки "on" с большой буквы
- Имена методов подписанных на события (Action) начинаются с приставки "On" с большой буквы
- Имена методов большими буквами
- Название перечислений (enumerator) начинается с большой буквы E например "ECharacterType"
- Переменные перечеслений (enumerator) начинаются с большой буквы
- Публичные свойства начинаются с Большой буквы
- Публичные переменные с маленькой буквы без нижних подчёркиваний независимо от атрибутов
- Приватные переменные с атрибутом [SerializedField] начинаются с маленькой буквы без нижнего подчёркивания
- Массивы и списки начинаются с маленькой буквы
- Приватные переменные объявляемые в классе без атрибута начинаются с нижнего подчёркивания: _variable
- Приватные переменные объявляемые в в методе класса начинаются с маленькой буквы: variable
- Приватные Zenject переменные с нижнего подчёркивания: _inputService
- Имена префабов в project вне кода в стиле CamelCAse

# Последовательность расположения полей

Свойства распологаются рядом со свойствами, переменные рядом с переменными.
```
Public
[SerializedField] Private
[HideInInspector] Public
Делегаты/события Action
Свойства Public
Обычная переменная public
Обычная переменная Private
Private переменные используемые в zenject
```
# Порядок расположения методов

```
Сначала OnEnable
Потом 	OnDisable
Потом 	Start
```

# Примечание
- У всех переменных указывать явно модификатор доступа будь то Private или Public.
- Все события и делегаты вызываются с помощью .Invoke для точто чтобы не спутать их с методом.
- Оператор условного null старатся не использовать так как он настолько маленький что его можно пропутсить.
- Все события "Action" вызываются методом из того же класса в котором это событие описанно
- Названия веток и коммитов веток в GIT по правилам snake_case

# Форматирование кода в Discord
```  ``` Discord: чтобы выделить текст в введите его между обратными апострофами