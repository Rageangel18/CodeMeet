IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'app_read' AND type = 'R')
    CREATE ROLE [app_read];

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'app_write' AND type = 'R')
    CREATE ROLE [app_write];

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'app_admin' AND type = 'R')
    CREATE ROLE [app_admin];



-- read может смотреть отчёты и сводки, но не менять данные
GRANT SELECT ON OBJECT::[dbo].[vSessionList]       TO [app_read];
GRANT SELECT ON OBJECT::[dbo].[vCandidateFeedback] TO [app_read];
GRANT EXECUTE ON OBJECT::[dbo].[sp_GetSessionSummary] TO [app_read];

-- write может работать с основными таблицами через приложение
GRANT SELECT, INSERT, UPDATE ON OBJECT::[dbo].[sessions]     TO [app_write];
GRANT SELECT, INSERT, UPDATE ON OBJECT::[dbo].[participants] TO [app_write];
GRANT SELECT, INSERT, UPDATE ON OBJECT::[dbo].[feedback]     TO [app_write];
GRANT SELECT, INSERT, UPDATE ON OBJECT::[dbo].[exec_requests] TO [app_write];

-- admin может выполнять SP по созданию сессий и фидбэка напрямую
GRANT EXECUTE ON OBJECT::[dbo].[sp_CreateSessionWithParticipants] TO [app_admin];
GRANT EXECUTE ON OBJECT::[dbo].[sp_AddFeedback]                  TO [app_admin];
