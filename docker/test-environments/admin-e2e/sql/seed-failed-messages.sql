IF NOT EXISTS (SELECT 1 FROM dbo.FailedMessages WHERE SourceId = 'E2E-TRANSIENT-001')
BEGIN
    INSERT INTO dbo.FailedMessages
        (Id, SourceId, Payload, ErrorMessage, FailureType, CreatedAt, LastAttemptAt)
    VALUES
        ('A1000000-0000-0000-0000-000000000001',
         'E2E-TRANSIENT-001',
         '{"sourceId":"E2E-TRANSIENT-001","data":{"customerId":1001,"name":"Transient Test Customer","amount":125.50,"status":"Pending"}}',
         'System B temporarily unavailable.',
         1,
         SYSUTCDATETIME(),
         SYSUTCDATETIME());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.FailedMessages WHERE SourceId = 'E2E-PERMANENT-001')
BEGIN
    INSERT INTO dbo.FailedMessages
        (Id, SourceId, Payload, ErrorMessage, FailureType, CreatedAt, LastAttemptAt)
    VALUES
        ('A1000000-0000-0000-0000-000000000002',
         'E2E-PERMANENT-001',
         '{"sourceId":"E2E-PERMANENT-001","data":{"customerId":1002,"name":"Permanent Test Customer","amount":250.00,"status":"Invalid"}}',
         'The message was rejected by downstream validation.',
         2,
         SYSUTCDATETIME(),
         SYSUTCDATETIME());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.FailedMessages WHERE SourceId = 'E2E-CORRECT-PAYLOAD-001')
BEGIN
    INSERT INTO dbo.FailedMessages
        (Id, SourceId, Payload, ErrorMessage, FailureType, CreatedAt, LastAttemptAt)
    VALUES
        ('A1000000-0000-0000-0000-000000000003',
         'E2E-CORRECT-PAYLOAD-001',
         '{"sourceId":"E2E-CORRECT-PAYLOAD-001","data":{"customerId":-1,"name":"Payload Correction Customer","amount":75.25,"status":"Pending"}}',
         'Customer ID is invalid. Correct the payload and replay.',
         2,
         SYSUTCDATETIME(),
         SYSUTCDATETIME());
END;