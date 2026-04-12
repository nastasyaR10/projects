-- Триггер для проверки одного активного сбора на ребенка
CREATE OR REPLACE FUNCTION check_single_active_fundraising()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.Flag_end = FALSE THEN
        IF EXISTS (
            SELECT 1 
            FROM Fundraising 
            WHERE ID_child = NEW.ID_child 
            AND Flag_end = FALSE
            AND ID_fundraising <> COALESCE(NEW.ID_fundraising, 0)
        ) THEN
            RAISE EXCEPTION 
                'У ребенка с ID=% уже есть активная акция по сбору средств. ' ||
                'Сначала завершите текущую акцию (установите Flag_end=TRUE).',
                NEW.ID_child;
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_check_single_active_fundraising
BEFORE INSERT OR UPDATE ON Fundraising
FOR EACH ROW
EXECUTE FUNCTION check_single_active_fundraising();

-- Триггер для обновления флага при изменениях в Donation
CREATE OR REPLACE FUNCTION update_flag_on_donation_change()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE Fundraising f
    SET Flag_end = (
        SELECT COALESCE(SUM(Sum_donation), 0) >= f.Sum
        FROM Donation d
        WHERE d.ID_fundraising = f.ID_fundraising
    )
    WHERE f.ID_fundraising = COALESCE(NEW.ID_fundraising, OLD.ID_fundraising);
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_donation_update_flag
AFTER INSERT OR UPDATE OR DELETE ON Donation
FOR EACH ROW
EXECUTE FUNCTION update_flag_on_donation_change();

-- Триггер для проверки и установки флага при вставке/обновлении Fundraising
CREATE OR REPLACE FUNCTION set_flag_on_fundraising_change()
RETURNS TRIGGER AS $$
DECLARE
    current_sum DECIMAL(10,2);
BEGIN
    SELECT COALESCE(SUM(Sum_donation), 0)
    INTO current_sum
    FROM Donation
    WHERE ID_fundraising = NEW.ID_fundraising;
    IF NEW.Flag_end = TRUE AND current_sum < NEW.Sum THEN
        RAISE EXCEPTION 'Cannot set Flag_end to TRUE: collected % < target %', 
                        current_sum, NEW.Sum;
    END IF;
    NEW.Flag_end := (current_sum >= NEW.Sum);
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_fundraising_set_flag
BEFORE INSERT OR UPDATE ON Fundraising
FOR EACH ROW
EXECUTE FUNCTION set_flag_on_fundraising_change();