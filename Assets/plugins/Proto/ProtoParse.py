import os
import shutil
cs_output_dir="Proto2C#Files"
proto_source_dir="ProtoFiles"

def get_filelist():
    path = os.getcwd() + os.sep
    path+=proto_source_dir
    return os.listdir(path)
    

def proto_parse():
    Filelist = get_filelist()
    for file in Filelist:
        print(file)
        if file.endswith('.proto'):
            csPath = os.getcwd() + os.sep + cs_output_dir
            if not os.path.exists(csPath):
                os.makedirs(csPath)
            protoRPath = file.replace(os.getcwd(), "")
            stringCmd ="protoc " +  " --csharp_out=./" + cs_output_dir  + os.sep +" "+"./"+proto_source_dir+os.sep+protoRPath
            os.system(stringCmd)
            print(stringCmd)



if __name__ =="__main__":
    proto_parse()