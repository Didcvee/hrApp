import pandas as pd
from sqlalchemy import create_engine, text
import re
from datetime import datetime

# Настройка подключения
engine = create_engine("postgresql+psycopg2://postgres:2121@localhost:5432/hr")

# Чтение Excel
df = pd.read_excel("data/orgStructure.xlsx", header=None)
df = df[7:].reset_index(drop=True)

# Получение маппинга названий в id
def get_id_map(table, conn):
    rows = conn.execute(text(f"SELECT id, name FROM {table}")).fetchall()
    return {r.name.strip(): r.id for r in rows}

with engine.begin() as conn:
    position_map = get_id_map("position", conn)
    department_map = get_id_map("department", conn)

# Регулярка на ФИО (фамилия имя отчество)
fio_regex = re.compile(r"^([А-ЯЁа-яё]+)\s+([А-ЯЁа-яё]+)\s+([А-ЯЁа-яё]+)$")

# Переменные для текущего контекста
current_department = None
employees = []

for idx, row in df.iterrows():
    first_col, second_col = row[0], row[1]

    if isinstance(first_col, str) and re.match(r"^\d+(\.\d+)*\.\s", first_col):
        current_department = first_col.strip()
        continue

    if pd.notna(first_col) and pd.notna(second_col):
        # Должность и ФИО
        position_name = first_col.strip()
        fio = second_col.strip()

        match = fio_regex.match(fio)
        if not match:
            continue  # пропускаем странные записи

        lastname, firstname, patronymic = match.groups()

        # Остальные поля
        birthdate = row[2] if not pd.isna(row[2]) else None
        workphone = str(row[3]).strip() if not pd.isna(row[3]) else None
        officeroom = str(row[4]).strip() if not pd.isna(row[4]) else None
        corporateemail = str(row[5]).strip() if not pd.isna(row[5]) else None

        # id-шники
        position_id = position_map.get(position_name)
        _, _, result = current_department.partition(' ')
        department_id = department_map.get(result)

        if position_id is None or department_id is None:
            print(f"Пропущено: {position_name=} {current_department=}")
            continue

        employees.append({
            "firstname": firstname,
            "lastname": lastname,
            "patronymic": patronymic,
            "birthdate": birthdate,
            "workphone": workphone,
            "officeroom": officeroom,
            "corporateemail": corporateemail,
            "positionid": position_id,
            "departmentid": department_id
        })

# Вставка в БД
with engine.begin() as conn:
    for emp in employees:
        conn.execute(text("""
            INSERT INTO employee (
                firstname, lastname, patronymic,
                birthdate, workphone, officeroom, corporateemail,
                departmentid, positionid
            ) VALUES (
                :firstname, :lastname, :patronymic,
                :birthdate, :workphone, :officeroom, :corporateemail,
                :departmentid, :positionid
            )
        """), emp)
