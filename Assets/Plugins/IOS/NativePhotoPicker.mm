#import <UIKit/UIKit.h>
#import <Photos/Photos.h>
#import "UnityInterface.h" 

// Unity 回调函数类型定义
typedef void (*GallerySaveCallback)(void* context, const char *result);
typedef void (*ImagePickCallback)(void* context, const char *base64Image);

@interface NativePhotoPicker : NSObject<UIImagePickerControllerDelegate, UINavigationControllerDelegate>
+ (instancetype)shared;
- (void)saveImageToGallery:(NSString *)filePath callback:(GallerySaveCallback)callback;
- (void)pickImageWithEditing:(BOOL)allowEditing callback:(ImagePickCallback)callback;
@end

@implementation NativePhotoPicker {
    void* _saveContext;
    void* _pickContext;
    GallerySaveCallback _saveCallback;
    ImagePickCallback _pickCallback;
    UIViewController *_unityViewController;
    UIImagePickerController *_imagePicker;
}

+ (instancetype)shared {
    static NativePhotoPicker *instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[NativePhotoPicker alloc] init];
    });
    return instance;
}

//---------------- 保存图片到相册 -----------------
- (void)saveImageToGallery:(NSString *)filePath 
                    callback:(GallerySaveCallback)callback
                    context:(void*)context 
{
    _saveCallback = callback;
    _saveContext = context; // 需要新增成员变量 void* _saveContext;
    
    UIImage *image = [UIImage imageWithContentsOfFile:filePath];
    if (image == nil) {
        if (_saveCallback) _saveCallback(_saveContext, "error_image_load_failed");
        return;
    }
    
    [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
        if (status == PHAuthorizationStatusAuthorized) {
            dispatch_async(dispatch_get_main_queue(), ^{
                UIImageWriteToSavedPhotosAlbum(image, self, @selector(image:didFinishSavingWithError:contextInfo:), nil);
            });
        } else {
            if (_saveCallback) _saveCallback(_saveContext, "error_permission_denied");
        }
    }];
}

- (void)image:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo{
    if (_saveCallback) _saveCallback(_saveContext, error ? "error_save_failed" : "success");
}


//---------------- 从相册选择图片 -----------------
- (void)pickImageWithEditing:(BOOL)allowEditing callback:(ImagePickCallback)callback 
    context:(void*)context
{
    _pickCallback = callback;
    _pickContext = context; // 存储上下文
    _unityViewController = UnityGetGLViewController();
    
    _imagePicker = [[UIImagePickerController alloc] init];
    _imagePicker.sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
    _imagePicker.allowsEditing = allowEditing;
    _imagePicker.delegate = self;
    
    [_unityViewController presentViewController:_imagePicker animated:YES completion:nil];
}

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary<NSString *,id> *)info {
    UIImage *image = info[UIImagePickerControllerEditedImage] ?: info[UIImagePickerControllerOriginalImage];
    NSData *pngData = UIImagePNGRepresentation(image);
    NSString *base64String = [pngData base64EncodedStringWithOptions:0];
    
    if (_pickCallback) _pickCallback(_pickContext, [base64String UTF8String]);
    [picker dismissViewControllerAnimated:YES completion:nil];
}

@end

//---------------- C 函数接口（供 Unity 调用）----------------
extern "C" {
    // 保存图片到相册
    void _NativeGallery_SaveImage(const char *filePath, GallerySaveCallback callback, void* context) {
        [[NativePhotoPicker shared] saveImageToGallery:[NSString stringWithUTF8String:filePath] callback:callback context:context];
    }

    // 从相册选择图片
    void _NativeGallery_PickImage(BOOL allowEditing, ImagePickCallback callback, void* context) {
        [[NativePhotoPicker shared] pickImageWithEditing:allowEditing callback:callback context:context];
    }
}