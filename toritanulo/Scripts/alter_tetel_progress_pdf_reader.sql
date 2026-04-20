ALTER TABLE tetel_olvasasi_allapotok
  ADD COLUMN IF NOT EXISTS last_page INT NOT NULL DEFAULT 1 AFTER haladas_szazalek,
  ADD COLUMN IF NOT EXISTS scroll_progress DECIMAL(6,5) NOT NULL DEFAULT 0 AFTER last_page,
  ADD COLUMN IF NOT EXISTS page_count INT NOT NULL DEFAULT 0 AFTER scroll_progress,
  ADD COLUMN IF NOT EXISTS completed TINYINT(1) NOT NULL DEFAULT 0 AFTER page_count;

UPDATE tetel_olvasasi_allapotok
SET
  last_page = CASE WHEN last_page < 1 THEN 1 ELSE last_page END,
  scroll_progress = CASE
    WHEN scroll_progress < 0 THEN 0
    WHEN scroll_progress > 1 THEN 1
    ELSE scroll_progress
  END,
  completed = CASE WHEN haladas_szazalek >= 100 THEN 1 ELSE completed END;
