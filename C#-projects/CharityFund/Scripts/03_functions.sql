--Функции для отчётов
--Список активных/завершённых акций
CREATE FUNCTION get_active_fundraisings(is_active BOOLEAN)
RETURNS TABLE (
    "ID акции" INT,
    "Название болезни" VARCHAR,
    "ФИО ребёнка" VARCHAR,
    "Целевая сумма" DECIMAL,
    "Собранная сумма" DECIMAL,
    "Дата начала" DATE
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        f.id_fundraising::INT AS "ID акции",
        d.name::VARCHAR AS "Название болезни",
        c.full_name::VARCHAR AS "ФИО ребёнка",
        f.sum::DECIMAL AS "Целевая сумма",
        COALESCE(SUM(don.sum_donation), 0)::DECIMAL AS "Собранная сумма",
        f.date_begin::DATE AS "Дата начала"
    FROM Fundraising f
    LEFT JOIN Disease d ON d.ID_disease = f.ID_disease
    LEFT JOIN Child c ON f.ID_child = c.ID_child
    LEFT JOIN donation don ON f.id_fundraising = don.id_fundraising
    WHERE f.flag_end = NOT is_active
    GROUP BY 
        f.id_fundraising,
        d.name,
        c.full_name,
        f.sum,
        f.date_begin
    ORDER BY f.ID_Fundraising;
END;
$$ LANGUAGE plpgsql;

-- Список болезней в порядке убывания частоты встречаемости
CREATE OR REPLACE FUNCTION get_disease_statistic(
    t1 DECIMAL(10,2), 
    t2 DECIMAL(10,2)
)
RETURNS TABLE (
    ID_disease INT,
    "Название заболевания" VARCHAR(255),
    "Количество сборов" BIGINT,
    "Средняя сумма сбора" DECIMAL(10,2),
    "Общая сумма сборов" DECIMAL(10,2),
    "Процент от всех сборов (%)" DECIMAL(5,2),
    "Завершенных сборов" BIGINT,
    "Активных сборов" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d.ID_disease,
        d.Name as "Название заболевания",
        COUNT(f.ID_fundraising) as "Количество сборов",
        COALESCE(ROUND(AVG(f.Sum), 2), 0) as "Средняя сумма сбора",
        ROUND(SUM(f.Sum), 2) as "Общая сумма сборов",
        ROUND(COUNT(f.ID_fundraising) * 100.0 / (SELECT COUNT(*) FROM Fundraising), 2) as "Процент от всех сборов (%)",
        SUM(CASE WHEN f.Flag_end = TRUE THEN 1 ELSE 0 END) as "Завершенных сборов",
        SUM(CASE WHEN f.Flag_end = FALSE THEN 1 ELSE 0 END) as "Активных сборов"
    FROM Disease d
    LEFT JOIN Fundraising f ON d.ID_disease = f.ID_disease
    GROUP BY d.ID_disease, d.Name
    HAVING COALESCE(ROUND(AVG(f.Sum), 2), 0) BETWEEN t1 AND t2
    ORDER BY COUNT(f.ID_fundraising) DESC, d.Name;
END;
$$ LANGUAGE plpgsql;

-- Список сборов, в которых имеются пожертвования из иностранных государств
CREATE OR REPLACE FUNCTION get_foreign_donations_statistic(
    t1 DECIMAL(10,2), 
    t2 DECIMAL(10,2)
)
RETURNS TABLE (
    "ID акции" INT,
    "Ребенок" VARCHAR(255),
    "Заболевание" VARCHAR(255),
    "Целевая сумма" DECIMAL(10,2),
    "Дата начала" DATE,
    "Иностранные страны" TEXT,
    "Кол-во иностранных пожертвований" BIGINT,
    "Сумма иностранных пожертвований" DECIMAL(10,2)
) AS $$
BEGIN
    RETURN QUERY
    WITH country_list AS (
        SELECT 
            don.ID_fundraising,
            STRING_AGG(DISTINCT cntry.Name, ', ') as countries
        FROM Donation don
        JOIN Country cntry ON don.ID_country = cntry.ID_country
        WHERE cntry.Name != 'Россия'
        GROUP BY don.ID_fundraising
    )
    SELECT DISTINCT
        f.ID_fundraising as "ID акции",
        c.Full_name as "Ребенок",
        d.Name as "Заболевание",
        f.Sum as "Целевая сумма",
        f.Date_begin as "Дата начала",
        COALESCE(cl.countries, 'Нет иностранных пожертвований') as "Иностранные страны",
        COUNT(DISTINCT don.ID_donation) as "Кол-во иностранных пожертвований",
        COALESCE(SUM(don.Sum_donation), 0) as "Сумма иностранных пожертвований"
    FROM Fundraising f
    LEFT JOIN Donation don ON f.ID_fundraising = don.ID_fundraising
    LEFT JOIN Country cntry ON don.ID_country = cntry.ID_country AND cntry.Name != 'Россия'
    JOIN Child c ON f.ID_child = c.ID_child
    JOIN Disease d ON f.ID_disease = d.ID_disease
    LEFT JOIN country_list cl ON f.ID_fundraising = cl.ID_fundraising
    GROUP BY f.ID_fundraising, c.Full_name, d.Name, f.Sum, f.Date_begin, cl.countries
    HAVING COUNT(DISTINCT don.ID_donation) > 0 AND f.sum BETWEEN t1 AND t2
    ORDER BY COALESCE(SUM(don.Sum_donation), 0) DESC;
END;
$$ LANGUAGE plpgsql;