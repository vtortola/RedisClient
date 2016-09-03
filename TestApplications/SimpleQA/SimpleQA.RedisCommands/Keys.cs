using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQA
{
    public static class Keys
    {
        public static String QuestionCounter()
        {
            return "questioncounter";
        }
        public static String QuestionKey(String id)
        {
            return "question:" + id;
        }
        public static String UserVotesKey(String user)
        {
            return "user:" + user + ":votes";
        }
        public static String UserInboxKey(String user)
        {
            return "user:" + user + ":inbox";
        }
        public static String UserKey(String username)
        {
            return "user:" + username;
        }
        public static String QuestionTagsKey(String id)
        {
            return "question:" + id + ":tags";
        }
        public static String QuestionAnswerCollectionKey(String id)
        {
            return "question:" + id + ":answers";
        }
        public static String AnswerKey(String questionId, String answerId)
        {
            return "question:" + questionId + ":answer:" + answerId;
        }
        public static String AnswerCollectionKey(String questionId)
        {
            return "question:" + questionId + ":answers";
        }
        public static String QuestionCloseVotesCollectionKey(String questionId)
        {
            return "question:" + questionId + ":closevotes";
        }
        public static String QuestionAnswerIdStore(String questionId)
        {
            return "question:" + questionId + ":answers_ids";
        }
        internal static String QuestionIdStore()
        {
            return "questions_ids";
        }
        internal static String AnswerVoteKey(String questionId, String answerId)
        {
            return "question:" + questionId + ":answer:" + answerId +":votes";
        }
        internal static String GenerateUserSession()
        {
            return "user:session:" + Guid.NewGuid().ToString().Replace("-", String.Empty);
        }
        internal static String QuestionsByDate()
        {
            return "questions:bydate";
        }
        internal static String QuestionsByScore()
        {
            return "questions:byscore";
        }
        internal static String TagsByScore()
        {
            return "tags:byscore";
        }
        internal static String TagsByDate()
        {
            return "tags:bydate";
        }

        internal static String TagCounting()
        {
            return "tag:counting";
        }
        internal static String AutoCompleteTags()
        {
            return "tags:autocomplete";
        }
        internal static String TagKeyByScore(String tag)
        {
            return "tag:" + tag + ":byscore";
        }

        internal static String TagKeyByDate(String tag)
        {
            return "tag:" + tag + ":bydate";
        }

        public static String QuestionNotification(String questionId)
        {
            return QuestionKey(questionId);
        }

        public static String TagNotification(String tag)
        {
            return "tag:" + tag;
        }
     }
}