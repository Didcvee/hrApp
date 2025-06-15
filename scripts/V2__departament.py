import pandas as pd
import re
from sqlalchemy import create_engine, text

file_path = "data/orgStructure.xlsx"
db_url = "postgresql+psycopg2://postgres:2121@localhost:5432/hr"

df = pd.read_excel(file_path, header=None)

department_rows = df[0].dropna()
department_rows = department_rows[department_rows.astype(str).str.match(r"^\d+(\.\d+)*\.\s")]

departments = []
for row in department_rows:
    match = re.match(r"^(\d+(?:\.\d+)*\.)\s+(.*)$", str(row))
    if not match:
        continue
    code = match.group(1).rstrip('.')
    name = match.group(2).strip()
    parent_code = '.'.join(code.split('.')[:-1]) if '.' in code else None
    departments.append({'code': code, 'name': name, 'parent_code': parent_code})

df_dep = pd.DataFrame(departments)
df_dep = df_dep.merge(
    df_dep[['code', 'name']].rename(columns={'code': 'parent_code', 'name': 'parent_name'}),
    on='parent_code',
    how='left'
)
df_dep = df_dep[['name', 'parent_name']]

engine = create_engine(db_url)

with engine.begin() as conn:
    inserted = {}

    for _, row in df_dep.iterrows():
        name = row['name']
        if name in inserted:
            continue

        existing = conn.execute(
            text("SELECT id FROM department WHERE name = :name"),
            {"name": name}
        ).scalar()

        if existing:
            inserted[name] = existing
        else:
            new_id = conn.execute(
                text("INSERT INTO department (name) VALUES (:name) RETURNING id"),
                {"name": name}
            ).scalar()
            inserted[name] = new_id

    for _, row in df_dep.dropna(subset=['parent_name']).iterrows():
        child = row['name']
        parent = row['parent_name']
        conn.execute(
            text("UPDATE department SET parentid = :pid WHERE name = :child"),
            {"pid": inserted[parent], "child": child}
        )

print("✅ Подразделения успешно импортированы в таблицу `department`.")
