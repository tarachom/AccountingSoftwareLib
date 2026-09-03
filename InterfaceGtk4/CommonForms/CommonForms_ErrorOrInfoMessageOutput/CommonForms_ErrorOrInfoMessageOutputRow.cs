using Gtk;
using GObject;
using AccountingSoftware;
using GdkPixbuf;

namespace InterfaceGtk4;

[Subclass<ListBoxRow>("CommonForms_ErrorOrInfoMessageOutputRow")]
[Template<AssemblyResource>("CommonForms_ErrorOrInfoMessageOutputRow.ui")]
public partial class CommonForms_ErrorOrInfoMessageOutputRow : ListBoxRow
{
    [Connect("image")] Image image;
    [Connect("box_text")] Box boxText;
    [Connect("label_datetime")] Label labelDateTime;
    [Connect("label_process")] Label labelProcess;
    [Connect("label_caption")] Label labelCaption;
    [Connect("label_message")] Label labelMessage;
    [Connect("button_delete")] Button buttonDelete;

    public static CommonForms_ErrorOrInfoMessageOutputRow NewWithData(Dictionary<string, object> row, CommonForms_ErrorOrInfoMessageOutput owner)
    {
        CommonForms_ErrorOrInfoMessageOutputRow listRow = NewWithProperties([]);
        listRow.SetData(row, owner);

        return listRow;
    }

    /// <summary>
    /// Заповнення рядка
    /// </summary>
    public void SetData(Dictionary<string, object> row, CommonForms_ErrorOrInfoMessageOutput owner)
    {
        Pixbuf? imagePixBuff = row["message_type"] switch
        {
            'E' => Icon.ForInformationBig.Error,
            'I' => Icon.ForInformationBig.Ok,
            'F' => Icon.ForInformationBig.File,
            _ => Icon.ForInformationBig.Error
        };

        image.SetFromPixbuf(imagePixBuff);

        labelDateTime.SetMarkup($"<i>{row["date"]}</i>");
        labelProcess.SetMarkup($"<i>{row["process"]}</i>");
        labelCaption.SetMarkup($"<b>{row["name"]}</b>");
        labelMessage.SetMarkup($"{row["message"]}");

        //Для відкриття
        {
            UniqueID uniqueID = new(row["uid"]);
            string type = row["type"].ToString() ?? "";

            if (!uniqueID.IsEmpty() && !string.IsNullOrEmpty(type))
                boxText.Append(owner.CreateCompositeControl("", new UuidAndText(uniqueID, type)));
        }

        buttonDelete.OnClicked += async (_, _) => await owner.Remove(this, (int)row["pkey"]);
    }
}