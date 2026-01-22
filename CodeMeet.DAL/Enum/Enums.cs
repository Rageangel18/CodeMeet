namespace CodeMeet.DAL.Enums;

public enum SessionStatus
{
    Draft,
    Scheduled,
    Live,
    Completed,
    Cancelled
}

public enum ParticipantRole
{
    Interviewer,
    Candidate,
    Observer
}

public enum Recommendation
{
    Hire,
    Hold,
    Reject
}

public enum ExecStatus
{
    Queued,
    Running,
    Completed,
    Failed
}

public enum QuestionLevel
{
    Easy,
    Medium,
    Hard
}

public enum CodeLanguage
{
    CSharp,
    Javascript,
    Plaintext
}

public enum OrgRole
{
    OrgAdmin,
    Recruiter,
    Interviewer,
    HiringManager
}
