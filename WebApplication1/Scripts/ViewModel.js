function ResizeSettingsClass(Name, Width, Height) {
    var self = this;
    self.Name = Name;
    self.Width = Width;
    self.Height = Height;
}

function ImageModel(o) {
    var self = this;
    self.Id = o.Id;
    self.FilePath = o.FilePath;
    self.FileName = o.FileName;
    self.PreviewPath = o.PreviewPath;
    self.Width = o.Width;
    self.Height = o.Height;
    self.ResizedImages = ko.observableArray();
    if (o.ResizedImages != null) {
        for (var i = 0; i < o.ResizedImages.length; i++) {
            self.ResizedImages.push(new ResizedImageModel(o.ResizedImages[i]));
        }
    }
}

function ResizedImageModel(o) {
    var self = this;
    self.Id = o.Id;
    self.FilePath = o.FilePath;
    self.FileName = o.FileName;
    self.PreviewPath = o.PreviewPath;
    self.Width = o.Width;
    self.Height = o.Height;
    self.StartResize = o.StartTime;
    self.FinishResize = o.FinishTime;
}

function DBImagesTable(PageSize) {
    var self = this;
    self.PageSize = PageSize;
    self.CurrentPage = ko.observable(1);
    self.CurrentSortOrder = ko.observable("asc");
    self.CurrentSortColumn = ko.observable("Id");
    self.Pages = ko.observableArray();
    self.TotalPages = ko.observable(0);

    self.Columns = ko.observableArray(['Id', 'FileName', "Width", "Height", 'Id']);
    self.Orders = ko.observableArray(['asc', 'desc']);
    self.DBImages = ko.observableArray();

    self.CurrentSort = ko.computed(function () {
        return self.CurrentSortColumn() + self.CurrentSortOrder();
    }, self);

    self.SetSort = function (data, event) {
        if (self.CurrentSortColumn() == data) {
            self.CurrentSortOrder((self.CurrentSortOrder() == 'asc') ? 'desc' : 'asc');
        } else {
            self.CurrentSortOrder('asc');
            self.CurrentSortColumn(data);
        }
        self.GetImagesFromServer();
    }

    self.NextPage = function () {
        if ((self.CurrentPage() + 1) <= self.TotalPages())
            self.CurrentPage(self.CurrentPage() + 1); self.GetImagesFromServer();
    }
    self.PreviousPage = function () {
        if (((self.CurrentPage() - 1) > 0))
            self.CurrentPage(self.CurrentPage() - 1); self.GetImagesFromServer();
    }
    self.SetPage = function (data, event) {
        if (self.CurrentPage() != data) {
            self.CurrentPage(data);
            self.GetImagesFromServer();
        }
    }


    self.GetImagesFromServer = function () {
        var self = this;
        $.getJSON('odata/Images?$top=' + self.PageSize +
             ((self.CurrentPage() > 1) ? '&$skip=' + (self.CurrentPage() - 1) * self.PageSize : '') +
            '&$orderby=' + self.CurrentSortColumn() + ' ' + self.CurrentSortOrder() ,
             function (data) {
                 var Items = data["value"];
                 if (Items.length > 0) {
                     self.DBImages.removeAll();
                     for (var i = 0; i < Items.length; i++) {
                         self.DBImages.push(new ImageModel(Items[i]));
                     }
                     var totalpages = data["odata.count"] / self.PageSize;
                     self.TotalPages((data["odata.count"] % self.PageSize === 0) ? data["odata.count"] / self.PageSize
                         : 1+(data["odata.count"] - data["odata.count"] % self.PageSize) / self.PageSize);
                     self.Pages.removeAll();
                     for (var i = 1; i < self.TotalPages()+1; i++) {
                         self.Pages.push(i);
                     }
                 }
             });
    };

    self.GetImagesFromServer();
    return self;
}

function DBResizedImagesTable(PageSize) {
    var self = this;
    self.PageSize = PageSize;
    self.CurrentPage = ko.observable(1);
    self.CurrentSortOrder = ko.observable("asc");
    self.CurrentSortColumn= ko.observable("id");
    self.TotalPages = ko.observable(0);
    self.Pages = ko.observableArray();

    self.Columns = ko.observableArray(['id','name', "parent", "width", "height", "start", "finish", "parent"]);
    self.Orders = ko.observableArray(['asc','desc']);
    self.DBResizedImages = ko.observableArray();

    self.CurrentSort = ko.computed(function () {
        return self.CurrentSortColumn() + self.CurrentSortOrder();
    }, self);

    self.SetSort = function (data, event) {
        if (self.CurrentSortColumn() == data) {            
            self.CurrentSortOrder((self.CurrentSortOrder() == 'asc') ? 'desc' : 'asc');
        } else {
            self.CurrentSortOrder('asc');
            self.CurrentSortColumn(data);
        }
        self.GetResizedImagesFromServer();
    }

    self.NextPage = function () {
        if ((self.CurrentPage()+1) <= self.TotalPages())
            self.CurrentPage(self.CurrentPage()+1); self.GetResizedImagesFromServer();
    }
    self.PreviousPage = function () {
        if (((self.CurrentPage()-1) > 0) )
            self.CurrentPage(self.CurrentPage() - 1); self.GetResizedImagesFromServer();
    }
    self.SetPage = function (data, event) {
        if (self.CurrentPage() != data) {
            self.CurrentPage(data);
            self.GetResizedImagesFromServer();
        }
    }
    self.GetResizedImagesFromServer = function () {
        var self = this;
        $.getJSON('api/ResizedImages/' + self.CurrentPage() + '/' + self.CurrentSort(),
             function (data) {
                if (data.ListOfImages.length > 0) {
                    self.DBResizedImages.removeAll();
                    for (var i = 0; i < data.ListOfImages.length; i++) {
                        self.DBResizedImages.push(new ResizedImageModel(data.ListOfImages[i]));
                    }
                    self.TotalPages(data.TotalPages);
                    self.Pages.removeAll();
                    for (var i = 1; i < self.TotalPages() + 1; i++) {
                        self.Pages.push(i);
                    }
                }
            });
    };
    
    self.GetResizedImagesFromServer();
    return self;
}

function generateUUID() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
    return uuid;
};


function MainViewModel(PageSize)
{
    uuid = generateUUID();
        // size of set of images for server processing
    CountOfImages = ko.observable(10);
        //images are chose on client 
    Images = ko.observableArray();
    Name = ko.observable('_large');
    Width = ko.observable(1000);
    Height = ko.observable(1000);
    CustomResizeSettings = ko.observableArray();

    AddCustomResizeSetting = function () {
        var self = this;
        var ResizeSettingsModel = new ResizeSettingsClass(self.Name(), self.Width(), self.Height());
        $.ajax({
            url: 'Home/AddSetting' + '?uuid=' + self.uuid,
            type: "POST",
            data: ko.toJSON(ResizeSettingsModel),
            contentType: 'application/json; charset=utf-8',
            success: function (ResizeSettingsModel) {
                self.CustomResizeSettings.push(ResizeSettingsModel);
            },
            error: function (xhr, status, p3) {
                alert(xhr.responseText);
            }
        });
    };

    IsImageExsist = function (image) {
        var result = ko.utils.arrayFirst(this.Images(), function (item) {
            return item.name === image.name && item.size === image.size && item.type === image.type;
        });
        return (result != null) ? true : false
    };
        //Images from DB  
    DBImagesTable = new DBImagesTable(PageSize);
        //Resized images from DB  
    DBResizedImagesTable = new DBResizedImagesTable;
        //images are sent to server 
    ProcessingImages = ko.observableArray();
        //images are processed on server 
    ProcessedImages = ko.observableArray();
        // select images in input field
    SelectImages = function () {
        var files = document.getElementById('uploadFile').files;
        for (var i = 0; i < files.length; i++) {
            if (!this.IsImageExsist(files[i])) {
                this.Images.push(files[i]);
            }
            else { alert(files[i].name + " is already there"); }
        }

    };

    SentToServer = function () {
        var self = this;
        var formData = new FormData();
        var localCountOfImages = (self.Images().length > self.CountOfImages()) ? self.CountOfImages() : self.Images().length;
        for (var i = 0; i < localCountOfImages; i++) {
            var item = self.Images.shift();
            self.ProcessingImages.push(item);
            formData.append("file" + i, item)
        }
        $.ajax({
            url: 'Home/Upload' + '?uuid=' + self.uuid,
            type: "POST",
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        var image = new ImageModel(data[i]);
                        self.ProcessingImages.remove(function (item) { return item.name == image.FileName; });
                        self.ProcessedImages.push(image);
                    }
                }

            },
            error: function (xhr, status, p3) {
                alert(xhr.responseText);
                var length = self.ProcessingImages().length;
                for (var i = 0; i < length; i++) {
                    self.Images.push(self.ProcessingImages.shift());
                }
            }
        });
    };
    }

