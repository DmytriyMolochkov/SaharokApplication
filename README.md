# Автоматическое формирование PDF из редактируемых файлов разных форматов
---
Программа ориентирована на проектные организации и позволяет автоматически формировать PDF-альбомы и ZIP-архивы для разделов документаций проекта из редактируемых файлов форматов Word, Excel, AutoCAD, Kompas3D, NanoCAD.

Это клиент-серверное приложение. Клиентская часть представляет собой WPF приложение с применением паттерна MVVM. 
Формирование PDF-файлов происходит на машине пользователя при помощи установленных программ (Word, AutoCAD, ...), управление которыми осуществляется через API. 
Экземпляры классов программ создаются при помощи позднего связывания.

Серверная часть представляет собой консольное приложение. На сервер вынесена чать логики работы, чтобы без соединения с сервором пользоваться программой было невозможно.
Сервер также ведёт учёт пользователей и логирует их дейтсвия в БД.
Трафик между сервером и клиентов шифруется.

Клиентская часть может одновременно подключится к двум независимым серверам, которые резервируют друг друга.
Один сервер может прослушивать одновременно несколько независымых групп пользователей. Чтобы добавить новую группу, необходимо редактировать config-файл сервера.

Чтобы удалённо управлять работой серверов, было написано примитивное консольное приложение для администратора. 
  
