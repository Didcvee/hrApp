import pandas as pd
from sqlalchemy import create_engine, text

# Подключение к БД
engine = create_engine("postgresql+psycopg2://postgres:2121@localhost:5432/hr")

# Чтение Excel, без заголовков
df = pd.read_excel("data/orgStructure.xlsx", header=None)

# Пропускаем строки до реальных данных (вручную подбираем индекс)
df = df[7:].reset_index(drop=True)

# Подготовка списка должностей
positions = []
current_org = None
current_department = None

for idx, row in df.iterrows():
    first_col = row[0]
    second_col = row[1]

    if pd.notna(first_col) and pd.notna(second_col):
        # Строка с сотрудником — значит слева была должность
        position = row[0]
        if pd.notna(position):
            positions.append(position)

# Убираем дубли и пустые
unique_positions = sorted(set(filter(pd.notna, positions)))

print(unique_positions)

# Вставка в таблицу `position`
with engine.begin() as conn:
    for name in unique_positions:
        conn.execute(text("""
            INSERT INTO position (name)
            VALUES (:name)
        """), {"name": name})
