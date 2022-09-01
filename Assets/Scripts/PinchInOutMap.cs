using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchInOutMap : MonoBehaviour
{
    Map.MapController MapController;

 
    
    /// Root�I�u�W�F�N�g����MapController�̌Ăяo��
       void Start()
    {
        MapController = GameObject.Find("Root").GetComponent<Map.MapController>();
                     
    }

    //�ύX�\�ȃY�[���l
    private float MaxZoom = 22;
    private float MinZoom = 0;

    //���O��2�_�Ԃ̋���.
    private float backDist = 0.0f;

    //�����l
    private float vZoom = 17;


    /// �s���`�C���A�E�g��MapController��Zoom�l��ύX
    void Update()
    {
        // �}���`�^�b�`�ł��邱�Ƃ��m�F
        if (Input.touchCount >= 2)
        {
            // �^�b�`���Ă���2�_���擾
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            //�@2�_�^�b�`�J�n���̋������擾
            if (t2.phase == TouchPhase.Began)
            {
                backDist = Vector2.Distance(t1.position, t2.position);
            }
            else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
            {
                // �^�b�`�ʒu�̈ړ���A�������đ����A�O��̋�������̑��Βl�擾
                float newDist = Vector2.Distance(t1.position, t2.position);
                vZoom = vZoom + (newDist - backDist) / 1000.0f;

                // ���E�l���I�[�o�[�����ۂ̏���
                if (vZoom > MaxZoom)
                {
                    vZoom = MaxZoom;
                }
                else if (vZoom < MinZoom)
                {
                    vZoom = MinZoom;
                }

                // float��int�ɕϊ�
                MapController.Zoom = (int)vZoom;


            }
        }



    }
}
