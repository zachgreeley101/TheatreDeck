using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace theatredeck.app.core.api.notion.model
{
    public class NotionResponse
    {
        [JsonProperty("object")]
        public string? ObjectType { get; set; }

        [JsonProperty("results")]
        public List<Page>? Results { get; set; }

        [JsonProperty("next_cursor")]
        public string? NextCursor { get; set; }

        [JsonProperty("has_more")]
        public bool? HasMore { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("page_or_database")]
        public object? PageOrDatabase { get; set; }

        [JsonProperty("request_id")]
        public string? RequestId { get; set; }
    }

    public class Page
    {
        [JsonProperty("object")]
        public string? ObjectType { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("created_time")]
        public DateTime? CreatedTime { get; set; }

        [JsonProperty("last_edited_time")]
        public DateTime? LastEditedTime { get; set; }

        [JsonProperty("created_by")]
        public User? CreatedBy { get; set; }

        [JsonProperty("last_edited_by")]
        public User? LastEditedBy { get; set; }

        [JsonProperty("cover")]
        public object? Cover { get; set; }

        [JsonProperty("icon")]
        public object? Icon { get; set; }

        [JsonProperty("parent")]
        public Parent? Parent { get; set; }

        [JsonProperty("archived")]
        public bool? Archived { get; set; }

        [JsonProperty("in_trash")]
        public bool? InTrash { get; set; }

        [JsonProperty("properties")]
        public Properties? Properties { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("public_url")]
        public object? PublicUrl { get; set; }
    }

    public class User
    {
        [JsonProperty("object")]
        public string? ObjectType { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }
    }

    public class Parent
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("database_id")]
        public string? DatabaseId { get; set; }
    }

    public class Properties
    {
        [JsonProperty("IDSearch")]
        public FormulaProperty? IDSearch { get; set; }

        [JsonProperty("Server Drive")]
        public MultiSelectProperty? ServerDrive { get; set; }

        [JsonProperty("Created Date")]
        public CreatedTimeProperty? CreatedDate { get; set; }

        [JsonProperty("PlayNext")]
        public CheckboxProperty? PlayNext { get; set; }

        [JsonProperty("Year")]
        public NumberProperty? Year { get; set; }

        [JsonProperty("Volume")]
        public NumberProperty? Volume { get; set; }

        [JsonProperty("PlayCount")]
        public NumberProperty? PlayCount { get; set; }

        [JsonProperty("Start Time")]
        public NumberProperty? StartTime { get; set; }

        [JsonProperty("Skipping Time")]
        public RichTextProperty? SkippingTime { get; set; }

        [JsonProperty("End Time")]
        public NumberProperty? EndTime { get; set; }

        [JsonProperty("Modified Date")]
        public LastEditedTimeProperty? ModifiedDate { get; set; }

        [JsonProperty("Location")]
        public RichTextProperty? Location { get; set; }

        [JsonProperty("ID")]
        public UniqueIdProperty? ID { get; set; }

        [JsonProperty("Name")]
        public TitleProperty? Name { get; set; }

        [JsonProperty("Tags")]
        public MultiSelectProperty? Tags { get; set; }

        [JsonProperty("Collections")]
        public MultiSelectProperty? Collections { get; set; }

    }

    // ---- Property Types ----
    public class FormulaProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("formula")]
        public FormulaContent? Formula { get; set; }
    }
    public class FormulaContent
    {
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("string")]
        public string? String { get; set; }
    }

    public class MultiSelectProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("multi_select")]
        public List<MultiSelectOption>? MultiSelect { get; set; }
    }
    public class MultiSelectOption
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("color")]
        public string? Color { get; set; }
    }

    public class CreatedTimeProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("created_time")]
        public DateTime? CreatedTime { get; set; }
    }

    public class CheckboxProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("checkbox")]
        public bool? Checkbox { get; set; }
    }

    public class NumberProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("number")]
        public double? Number { get; set; }
    }

    public class RichTextProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("rich_text")]
        public List<RichTextContent>? RichText { get; set; }
    }
    public class RichTextContent
    {
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("text")]
        public TextContent? Text { get; set; }
        [JsonProperty("plain_text")]
        public string? PlainText { get; set; }
    }
    public class TextContent
    {
        [JsonProperty("content")]
        public string? Content { get; set; }
        [JsonProperty("link")]
        public object? Link { get; set; }
    }

    public class LastEditedTimeProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("last_edited_time")]
        public DateTime? LastEditedTime { get; set; }
    }

    public class UniqueIdProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("unique_id")]
        public UniqueId? UniqueId { get; set; }
    }
    public class UniqueId
    {
        [JsonProperty("prefix")]
        public string? Prefix { get; set; }
        [JsonProperty("number")]
        public int? Number { get; set; }
    }

    public class TitleProperty
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("title")]
        public List<TitleContent>? Title { get; set; }
    }
    public class TitleContent
    {
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("text")]
        public TextContent? Text { get; set; }
        [JsonProperty("plain_text")]
        public string? PlainText { get; set; }
        [JsonProperty("annotations")]
        public Annotations? Annotations { get; set; }
        [JsonProperty("href")]
        public string? Href { get; set; }
    }
    public class Annotations
    {
        [JsonProperty("bold")]
        public bool? Bold { get; set; }
        [JsonProperty("italic")]
        public bool? Italic { get; set; }
        [JsonProperty("strikethrough")]
        public bool? Strikethrough { get; set; }
        [JsonProperty("underline")]
        public bool? Underline { get; set; }
        [JsonProperty("code")]
        public bool? Code { get; set; }
        [JsonProperty("color")]
        public string? Color { get; set; }
    }
}
