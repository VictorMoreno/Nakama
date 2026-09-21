    create table chatbots (
        key uuid not null,
       Label varchar(255),
       Description text,
       Status int4,
       Context text,
       General boolean,
       Archived boolean,
       IconUrl varchar(255),
       Settings text,
       LastUsed timestamp,
       documents_json text,
       team_json text,
       primary key (key)
    );

    create table conversations (
        id uuid not null,
       bot_key uuid,
       Title varchar(255),
       StartedAt timestamp,
       exchanges_json text,
       primary key (id)
    );

    create table formations (
        key uuid not null,
       Title varchar(255),
       Audience varchar(255),
       Description text,
       Status int4,
       linked_chatbot_key uuid,
       Archived boolean,
       IconUrl varchar(255),
       MyProgress text,
       modules_json text,
       team_json text,
       primary key (key)
    );

    create table read_chatbots_list (
        key uuid not null,
       Label varchar(255),
       Description text,
       Status varchar(255),
       LastUsed timestamp,
       DocsCount int4,
       TeamCount int4,
       General boolean,
       Archived boolean,
       IconUrl varchar(255),
       primary key (key)
    );

    create table read_chatbot_detail (
        key uuid not null,
       Label varchar(255),
       Description text,
       Status varchar(255),
       LastUsed timestamp,
       Context text,
       General boolean,
       Archived boolean,
       IconUrl varchar(255),
       Socratic boolean,
       ExplicaMas boolean,
       Provider varchar(255),
       docs_json text,
       team_json text,
       gaps_json text,
       top_questions_json text,
       stats_json text,
       primary key (key)
    );

    create table read_chat_page (
        bot_key uuid not null,
       Label varchar(255),
       Context text,
       Socratic boolean,
       ExplicaMas boolean,
       Provider varchar(255),
       opening_json text,
       chips_json text,
       history_json text,
       primary key (bot_key)
    );

    create table read_formations_list (
        key uuid not null,
       Title varchar(255),
       Audience varchar(255),
       Description text,
       Status varchar(255),
       Archived boolean,
       IconUrl varchar(255),
       ProgressPct int4,
       ModulesCount int4,
       CompletedModulesCount int4,
       primary key (key)
    );

    create table read_formation_detail (
        key uuid not null,
       Title varchar(255),
       Audience varchar(255),
       Description text,
       Status varchar(255),
       Archived boolean,
       IconUrl varchar(255),
       linked_chatbot_key uuid,
       linked_chatbot_label varchar(255),
       modules_json text,
       team_json text,
       completed_module_ids_json text,
       exam_scores_json text,
       primary key (key)
    );

    create table read_formation_detail_chatbot_label_ref (
        bot_key uuid not null,
       Label varchar(255),
       primary key (bot_key)
    );

    create table read_analytics (
        key varchar(255) not null,
       ActiveBotsCount int4,
       AnsweredThisWeek int4,
       usage_by_bot_json text,
       primary key (key)
    );
